
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SAGM.Data.Entities;
using SAGM.Models;

namespace SAGM.Data
{
    public class SAGMContext : IdentityDbContext<User>
    {
        public SAGMContext(DbContextOptions<SAGMContext> options) : base(options) 
        { 
        }

        public DbSet<Country> Countries { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<City> Cities { get; set; }

        public DbSet<Category> Categories { get; set; }
        public DbSet<MaterialType> MaterialTypes { get; set; }
        public DbSet<Material> Materials { get; set; }

        public DbSet<Customer> Customers{ get; set; }

        public DbSet<Contact> Contacts { get; set; }

        public DbSet<Currency> Currencies { get; set; }

        public DbSet<Quote> Quotes { get; set; }

        public DbSet<QuoteDetail> QuoteDetails { get; set; }

        public DbSet<QuoteStatus> QuoteStatus { get; set; }


        public DbSet<Unit> Units { get; set; }


        internal static IConfiguration GetService(Type type)
        {
            throw new NotImplementedException();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Country>().HasIndex(c => c.CountryName).IsUnique();
            modelBuilder.Entity<State>().HasIndex("StateName","CountryId").IsUnique();
            modelBuilder.Entity<City>().HasIndex("CityName", "StateId").IsUnique();
            modelBuilder.Entity<Category>().HasIndex(c => c.CategoryName).IsUnique();
            modelBuilder.Entity<Customer>().HasIndex(c => c.CustomerName).IsUnique();
            modelBuilder.Entity<MaterialType>().HasIndex("MaterialTypeName", "CategoryId").IsUnique();
            modelBuilder.Entity<Material>().HasIndex("MaterialName", "MaterialTypeId").IsUnique();
            modelBuilder.Entity<Contact>().HasIndex("Name", "LastName", "CustomerId").IsUnique();
        }
    }
}
