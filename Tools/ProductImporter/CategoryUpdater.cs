using Microsoft.EntityFrameworkCore;
using SmartStock.Data;

namespace SmartStock.Tools;

public static class CategoryUpdater
{
    public static async Task RunAsync(SmartStockDbContext db)
    {
        var finalNames = new Dictionary<int, string>
        {
            [1] = "Kitchen Appliances",
            [2] = "Cookware",
            [3] = "Food Storage and Organization",
            [4] = "Utensils and Accessories"
        };

        var categoryMapping = new Dictionary<int, int>
        {
            [5] = 1, // Appliances
            [6] = 2, // Cookware
            [7] = 4, // Utensils
            [8] = 4, // Kitchen Accessories
            [9] = 3  // Food Storage
        };

        var utensilSkus = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase)
        {
            "SPN-1234", // Spoons
            "APP-23",   // Silver Spoons
            "UTE-001",  // Pastry Cutter
            "UTE-002",  // Vegetable Peelers
            "UTE-003",  // Wooden Spoon
            "UTE-004",  // Whisks
            "ACC-001"   // Silverware Set
        };

        var categories = await db.Categories
            .OrderBy(c => c.Id)
            .ToListAsync();

        var products = await db.Products
            .OrderBy(p => p.Id)
            .ToListAsync();

        Console.WriteLine("\nCATEGORY REORGANIZATION PREVIEW");
        Console.WriteLine("--------------------------------");

        foreach (var category in categories)
        {
            Console.WriteLine(
                $"{category.Id}: {category.Name}");
        }

        // Check that the database matches the preview
        // we previously reviewed.
        var expectedNames = new Dictionary<int, string>
        {
            [1] = "Small Kitchen Appliances",
            [2] = "Cookware and Utensils",
            [3] = "Food Storage and Organization",
            [4] = "Home Essentials and Accessories",
            [5] = "Appliances",
            [6] = "Cookware",
            [7] = "Utensils",
            [8] = "Kitchen Accessories",
            [9] = "Food Storage"
        };

        if (categories.Count != 9 ||
            categories.Any(c =>
                !expectedNames.TryGetValue(c.Id, out var name) ||
                c.Name != name))
        {
            Console.WriteLine(
                "\nSTOP: Categories have changed since our preview.");
            Console.WriteLine("No changes made.");
            return;
        }

        if (products.Count != 17 ||
            products.Any(p =>
                !finalNames.ContainsKey(p.CategoryId) &&
                !categoryMapping.ContainsKey(p.CategoryId)))
        {
            Console.WriteLine(
                "\nSTOP: Product count or category assignments changed.");
            Console.WriteLine("No changes made.");
            return;
        }

        var assignments = products.Select(p =>
        {
            int targetId = utensilSkus.Contains(p.Sku)
                ? 4
                : categoryMapping.TryGetValue(
                    p.CategoryId, out var mappedId)
                    ? mappedId
                    : p.CategoryId;

            return new
            {
                Product = p,
                TargetId = targetId
            };
        }).ToList();

        Console.WriteLine("\nFINAL PRODUCT CATEGORIES");

        foreach (var category in finalNames)
        {
            var assigned = assignments
                .Where(a => a.TargetId == category.Key)
                .ToList();

            Console.WriteLine(
                $"\n{category.Value} ({assigned.Count} products)");

            foreach (var item in assigned)
            {
                Console.WriteLine(
                    $"  {item.Product.Sku} - {item.Product.Name}");
            }
        }

        var expectedCounts = new Dictionary<int, int>
        {
            [1] = 5,
            [2] = 2,
            [3] = 3,
            [4] = 7
        };

        if (expectedCounts.Any(expected =>
            assignments.Count(a =>
                a.TargetId == expected.Key) != expected.Value))
        {
            Console.WriteLine(
                "\nSTOP: Product distribution does not match our plan.");
            Console.WriteLine("No changes made.");
            return;
        }

        Console.WriteLine(
            "\nThis will reassign products, rename categories 1-4,");
        Console.WriteLine(
            "and delete unused categories 5-9.");
        Console.WriteLine(
            "Product prices, quantities and images will not be edited.");

        Console.Write("\nType UPDATE to continue: ");

        if (Console.ReadLine() != "UPDATE")
        {
            Console.WriteLine("Cancelled. No changes made.");
            return;
        }

        await using var transaction =
            await db.Database.BeginTransactionAsync();

        try
        {
            // First, move products to the four retained IDs.
            foreach (var assignment in assignments)
            {
                assignment.Product.CategoryId =
                    assignment.TargetId;
            }

            await db.SaveChangesAsync();

            // Remove the five extra category records.
            var extras = categories
                .Where(c => c.Id >= 5)
                .ToList();

            db.Categories.RemoveRange(extras);
            await db.SaveChangesAsync();

            // Rename the four retained categories.
            foreach (var category in categories
                .Where(c => finalNames.ContainsKey(c.Id)))
            {
                category.Name = finalNames[category.Id];
            }

            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            Console.WriteLine(
                "\nSUCCESS! Four categories remain in Neon.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            Console.WriteLine(
                $"\nUpdate failed and was rolled back: {ex.Message}");
        }
    }
}

