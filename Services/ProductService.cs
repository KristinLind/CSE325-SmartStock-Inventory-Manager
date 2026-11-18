using Microsoft.EntityFrameworkCore;
using SmartStock.Data;
using SmartStock.Models;

namespace SmartStock.Services;

public record InventorySummary(
    int TotalProducts,
    int TotalUnits,
    decimal InventoryValue,
    int LowStockCount,
    int OutOfStockCount);

public class ProductService(IDbContextFactory<SmartStockDbContext> factory)
{
    // stock: "all", "low" (includes out of stock), or "out"
    public async Task<List<Product>> GetAllAsync(string? search = null, int categoryId = 0, string stock = "all")
    {
        await using var db = await factory.CreateDbContextAsync();
        var query = db.Products.Include(p => p.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(s) || p.Sku.ToLower().Contains(s));
        }

        if (categoryId > 0)
            query = query.Where(p => p.CategoryId == categoryId);

        if (stock == "low")
            query = query.Where(p => p.Quantity <= p.ReorderLevel);
        else if (stock == "out")
            query = query.Where(p => p.Quantity == 0);

        return await query.OrderBy(p => p.Name).ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Products.FindAsync(id);
    }

    public async Task SaveAsync(Product product)
    {
        await using var db = await factory.CreateDbContextAsync();

        product.Name = product.Name.Trim();
        product.Sku = product.Sku.Trim().ToUpperInvariant();

        var duplicate = await db.Products.AnyAsync(p => p.Sku == product.Sku && p.Id != product.Id);
        if (duplicate)
            throw new InvalidOperationException($"A product with SKU '{product.Sku}' already exists.");

        if (product.Id == 0)
            db.Products.Add(product);
        else
            db.Products.Update(product);

        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();

        if (await db.Sales.AnyAsync(s => s.ProductId == id))
            throw new InvalidOperationException(
                "This product has sales history and can't be deleted. Set its quantity to 0 instead.");

        await db.Products.Where(p => p.Id == id).ExecuteDeleteAsync();
    }

    public async Task<List<Product>> GetLowStockAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Products
            .Include(p => p.Category)
            .Where(p => p.Quantity <= p.ReorderLevel)
            .OrderBy(p => p.Quantity)
            .ThenBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<InventorySummary> GetSummaryAsync()
    {
        await using var db = await factory.CreateDbContextAsync();

        var totalProducts = await db.Products.CountAsync();
        var totalUnits = await db.Products.SumAsync(p => (int?)p.Quantity) ?? 0;
        var inventoryValue = await db.Products.SumAsync(p => (decimal?)p.Price * p.Quantity) ?? 0m;
        var lowStockCount = await db.Products.CountAsync(p => p.Quantity > 0 && p.Quantity <= p.ReorderLevel);
        var outOfStockCount = await db.Products.CountAsync(p => p.Quantity == 0);

        return new InventorySummary(
            TotalProducts: totalProducts,
            TotalUnits: totalUnits,
            InventoryValue: inventoryValue,
            LowStockCount: lowStockCount,
            OutOfStockCount: outOfStockCount);
    }
}