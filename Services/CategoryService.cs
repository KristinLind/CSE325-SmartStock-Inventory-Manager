using Microsoft.EntityFrameworkCore;
using SmartStock.Data;
using SmartStock.Models;

namespace SmartStock.Services;

public class CategoryService(IDbContextFactory<SmartStockDbContext> factory)
{
    public async Task<List<Category>> GetAllAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Categories.Include(c => c.Products).OrderBy(c => c.Name).ToListAsync();
    }

    public async Task AddAsync(Category category)
    {
        await using var db = await factory.CreateDbContextAsync();
        var name = category.Name.Trim();

        var exists = await db.Categories.AnyAsync(c => c.Name.ToLower() == name.ToLower());
        if (exists)
            throw new InvalidOperationException($"Category '{name}' already exists.");

        db.Categories.Add(new Category { Name = name });
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        var category = await db.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id);
        if (category is null) return;

        if (category.Products.Count > 0)
            throw new InvalidOperationException("This category still has products. Move or delete them first.");

        db.Categories.Remove(category);
        await db.SaveChangesAsync();
    }
}