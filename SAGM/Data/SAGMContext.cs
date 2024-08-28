
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

        public DbSet<Archive> Archives { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<City> Cities { get; set; }

        public DbSet<Category> Categories { get; set; }
        public DbSet<MaterialType> MaterialTypes { get; set; }
        public DbSet<Material> Materials { get; set; }

        public DbSet<Customer> Customers{ get; set; }

        public DbSet<Contact> Contacts { get; set; }

        public DbSet<Currency> Currencies { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderDetail> OrderDetails { get; set; }

        public DbSet<OrderComment> OrderComments { get; set; }

        public DbSet<OrderDetailComment> OrderDetailComments { get; set; }

        public DbSet<OrderStatus> OrderStatus { get; set; }

        public DbSet<QuoteStatus> QuoteStatus { get; set; }

        public DbSet<Quote> Quotes { get; set; }

        public DbSet<QuoteDetail> QuoteDetails { get; set; }


        public DbSet<Unit> Units { get; set; }

        public DbSet<QuoteComment> QuoteComments { get; set; }

        public DbSet<QuoteDetailComment> QuoteDetailComments { get; set; }

        public DbSet<Supplier> Suppliers { get; set; }

        public DbSet<WorkOrder> WorkOrders { get; set; }
        public DbSet<WorkOrderDetail> WorkOrderDetails { get; set; }

        public DbSet<WorkOrderStatus> WorkOrderStatus { get; set; }

        public DbSet<WorkOrderComment> WorkOrderComments { get; set; }
        public DbSet<WorkOrderDetailComment> WorkOrderDetailComments { get; set; }

        public DbSet<Process> Processes { get; set; }

        public DbSet<Machine> Machines { get; set; }

        public DbSet<WorkOrderDetailProcess> WorkOrderDetailProcesses { get; set; }

        public DbSet<WorkOrderDelivery> WorkOrderDeliveries { get; set; }

        public DbSet<WorkOrderDeliveryDetail> workOrderDeliveryDetails { get; set; }


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
            modelBuilder.Entity<Quote>().HasMany(q => q.QuoteComments).WithOne(q => q.Quote).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Process>().HasIndex(p => p.ProcessName).IsUnique();
            modelBuilder.Entity<Machine>().HasIndex("MachineName", "ProcessId").IsUnique();
        }
        public DbSet<SAGM.Data.Entities.Archive> Archive { get; set; }
    }
}
