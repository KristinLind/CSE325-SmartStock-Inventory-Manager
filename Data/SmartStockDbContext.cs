using Microsoft.EntityFrameworkCore;
using SmartStock.Models;

namespace SmartStock.Data;

public class SmartStockDbContext : DbContext
{
    public SmartStockDbContext(DbContextOptions<SmartStockDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Sale> Sales => Set<Sale>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // SKUs and category names must be unique
        b.Entity<Product>().HasIndex(p => p.Sku).IsUnique();
        b.Entity<Category>().HasIndex(c => c.Name).IsUnique();

        // Block deleting a category that has products, or a product that has sales
        b.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Sale>()
            .HasOne(s => s.Product)
            .WithMany()
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Starting categories from the team README
        b.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Small Kitchen Appliances" },
            new Category { Id = 2, Name = "Cookware and Utensils" },
            new Category { Id = 3, Name = "Food Storage and Organization" },
            new Category { Id = 4, Name = "Home Essentials and Accessories" }
        );
    }
}