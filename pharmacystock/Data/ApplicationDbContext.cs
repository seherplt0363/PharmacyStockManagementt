using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using pharmacystock.Models;

namespace PharmacyStock.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Brand> Brands { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<StockTransaction> StockTransactions { get; set; }

        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Supplier>().HasData(
                new Supplier { Id = 1, Name = "Selçuk Ecza Deposu", Email = "selcuk@example.com", Phone = "02165540300" },
                new Supplier { Id = 2, Name = "Hedef Ecza Deposu", Email = "hedef@example.com", Phone = "02165876000" },
                new Supplier { Id = 3, Name = "Alliance Healthcare", Email = "alliance@example.com", Phone = "02165642000" }
            );
        }
    }
}