using Carisma.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Stripe;
using DotNetEnv;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Identity configuration
builder.Services.AddDefaultIdentity<IdentityUser>(options => {
    options.SignIn.RequireConfirmedAccount = false; // Za lak�e testiranje
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// MVC services
builder.Services.AddControllersWithViews();

// Stripe configuration - OVDJE, NE U FUNKCIJI!
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];


//najnovije
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddRazorPages()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

//najnovije


// Build aplikacije
var app = builder.Build();

// Kreiranje defaultnih uloga i korisnika
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await CreateDefaultRolesAndUsers(services);
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();


//najnovije

var supportedCultures = new[] { new System.Globalization.CultureInfo("bs") };

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("bs"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

//najnovije

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run(); 

// Funkcija za kreiranje defaultnih uloga i korisnika
async Task CreateDefaultRolesAndUsers(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // Kreiranje uloga
    string[] roleNames = { "Administrator", "Podrska", "Korisnik" };
    foreach (var roleName in roleNames)
    {
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // Kreiranje podr�ka korisnika
    var podrskaUser = await userManager.FindByEmailAsync("podrska@gmail.com");
    if (podrskaUser == null)
    {
        var newUser = new IdentityUser()
        {
            UserName = "podrska@gmail.com",
            Email = "podrska@gmail.com",
            EmailConfirmed = true
        };
        var createResult = await userManager.CreateAsync(newUser, "Ooad2025!");
        if (createResult.Succeeded)
        {
            await userManager.AddToRoleAsync(newUser, "Podrska");
        }
    }
}