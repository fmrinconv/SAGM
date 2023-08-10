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
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Country>().HasIndex(c => c.CountryName).IsUnique();      
        }

    }
}
