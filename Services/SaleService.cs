using Microsoft.EntityFrameworkCore;
using SmartStock.Data;
using SmartStock.Models;

namespace SmartStock.Services;

public class SaleService(IDbContextFactory<SmartStockDbContext> factory)
{

public async Task<(List<Sale> Sales, decimal Total)> CheckoutAsync(IEnumerable<CartItem> cartItems)
{
    var lines = cartItems.ToList();
    if (lines.Count == 0)
        throw new InvalidOperationException("The cart is empty.");

    await using var db = await factory.CreateDbContextAsync();

    var ids = lines.Select(l => l.ProductId).ToList();
    var products = await db.Products
        .Where(p => ids.Contains(p.Id))
        .ToDictionaryAsync(p => p.Id);

    // Check every line first, so we never save half an order
    foreach (var line in lines)
    {
        if (!products.TryGetValue(line.ProductId, out var product))
            throw new InvalidOperationException($"'{line.Name}' no longer exists.");

        if (product.Quantity < line.Quantity)
            throw new InvalidOperationException(
                $"Only {product.Quantity} of '{product.Name}' in stock.");
    }

    // Reduce stock and record one sale per product, saved together in one transaction
    var soldAt = DateTime.Now;
    var sales = new List<Sale>();

    foreach (var line in lines)
    {
        var product = products[line.ProductId];
        product.Quantity -= line.Quantity;

        var sale = new Sale
        {
            ProductId = product.Id,
            QuantitySold = line.Quantity,
            UnitPrice = product.Price,   // current price, not the price when added
            SoldAt = soldAt
        };
        db.Sales.Add(sale);
        sales.Add(sale);
    }

    await db.SaveChangesAsync();
    return (sales, sales.Sum(s => s.Total));
}

    public async Task<Sale> RecordSaleAsync(int productId, int quantity)
    {
        if (quantity <= 0)
            throw new InvalidOperationException("Quantity must be at least 1.");

        await using var db = await factory.CreateDbContextAsync();

        var product = await db.Products.FindAsync(productId)
            ?? throw new InvalidOperationException("Product not found.");

        if (product.Quantity < quantity)
            throw new InvalidOperationException(
                $"Only {product.Quantity} of '{product.Name}' in stock.");

        // Reduce stock and save the sale together (one save = one transaction)
        product.Quantity -= quantity;

        var sale = new Sale
        {
            ProductId = product.Id,
            QuantitySold = quantity,
            UnitPrice = product.Price,
            SoldAt = DateTime.Now
        };
        db.Sales.Add(sale);
        await db.SaveChangesAsync();

        sale.Product = product;
        return sale;
    }

    public async Task<List<Sale>> GetRecentAsync(int count = 10)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Sales
            .Include(s => s.Product)
            .OrderByDescending(s => s.SoldAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<(int Count, decimal Revenue)> GetTodaySummaryAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        var today = DateTime.Today;

        var sales = await db.Sales.Where(s => s.SoldAt >= today).ToListAsync();
        return (sales.Count, sales.Sum(s => s.UnitPrice * s.QuantitySold));
    }
}