using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Carisma.Models;

namespace Carisma.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Osoba> Osoba { get; set; }
        public DbSet<Vozilo> Vozila { get; set; }
        public DbSet<Carisma.Models.Podrska> Podrska { get; set; } = default!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Osoba>().ToTable("Osoba");
            modelBuilder.Entity<Vozilo>().ToTable("Vozilo");
            modelBuilder.Entity<Rezervacija>().ToTable("Rezervacija");
            modelBuilder.Entity<Placanje>().ToTable("Placanje");
            modelBuilder.Entity<Podrska>().ToTable("Podrska");

            base.OnModelCreating(modelBuilder);


          
        }
        //public DbSet<Carisma.Models.Podrska> Podrska { get; set; } = default!;
        //public DbSet<Osoba> Osoba { get; set; }
        public DbSet<Rezervacija> Rezervacija { get; set; }

        

    }
}
