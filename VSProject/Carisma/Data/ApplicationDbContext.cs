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

        public DbSet<Osoba> Osobe { get; set; }
        public DbSet<Vozilo> Vozila { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Osoba>().ToTable("Osoba");
            modelBuilder.Entity<Vozilo>().ToTable("Vozilo");

            base.OnModelCreating(modelBuilder);
        }
    }
}
