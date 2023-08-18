using Microsoft.EntityFrameworkCore;
using SAGM.Data.Entities;

namespace SAGM.Data
{
    public class SAGMContext : DbContext
    {
        public SAGMContext(DbContextOptions<SAGMContext> options) : base(options) 
        { 
        }

        public DbSet<Country> Countries { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<City> Cities { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Country>().HasIndex(c => c.CountryName).IsUnique();
            modelBuilder.Entity<State>().HasIndex("StateName","CountryId").IsUnique();
            modelBuilder.Entity<City>().HasIndex("CityName", "StateId").IsUnique();
        }

    }
}
