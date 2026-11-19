using Microsoft.EntityFrameworkCore;
using SmartStock.Data;
using SmartStock.Models;

namespace SmartStock.Services;

public class CategoryService(IDbContextFactory<SmartStockDbContext> factory)
{
    public async Task<List<Category>> GetAllAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Categories
            .Include(c => c.Products)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task AddAsync(Category category)
    {
        await using var db = await factory.CreateDbContextAsync();
        var name = category.Name.Trim();

        var exists = await db.Categories.AnyAsync(
            c => c.Name.ToLower() == name.ToLower());

        if (exists)
            throw new InvalidOperationException(
                $"Category '{name}' already exists.");

        db.Categories.Add(new Category { Name = name });
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();

        var category = await db.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category is null) return;

        if (category.Products.Count > 0)
            throw new InvalidOperationException(
                "This category still has products. Move or delete them first.");

        db.Categories.Remove(category);
        await db.SaveChangesAsync();
    }

    // Rename a category without changing its ID or products.
    public async Task RenameAsync(int id, string newName)
    {
        await using var db = await factory.CreateDbContextAsync();

        var category = await db.Categories.FindAsync(id);

        if (category is null)
            throw new InvalidOperationException("Category not found.");

        newName = newName.Trim();

        if (string.IsNullOrWhiteSpace(newName))
            throw new InvalidOperationException(
                "Category name cannot be empty.");

        var exists = await db.Categories.AnyAsync(c =>
            c.Id != id &&
            c.Name.ToLower() == newName.ToLower());

        if (exists)
            throw new InvalidOperationException(
                $"Category '{newName}' already exists.");

        category.Name = newName;
        await db.SaveChangesAsync();
    }
}
