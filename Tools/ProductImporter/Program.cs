using Microsoft.EntityFrameworkCore;
using Npgsql;
using SmartStock.Data;
using SmartStock.Models;
using SmartStock.Tools;

// Locate the repository's existing .env file.
var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
string? envPath = null;

while (directory != null)
{
    var candidate = Path.Combine(directory.FullName, ".env");

    if (File.Exists(candidate))
    {
        envPath = candidate;
        break;
    }

    directory = directory.Parent;
}

if (envPath == null)
{
    Console.WriteLine("ERROR: Could not locate the project's .env file.");
    return;
}

var databaseUrl = File.ReadLines(envPath)
    .Select(line => line.Trim())
    .Where(line => line.StartsWith("DATABASE_URL="))
    .Select(line => line["DATABASE_URL=".Length..].Trim().Trim('"', '\''))
    .FirstOrDefault();

if (string.IsNullOrWhiteSpace(databaseUrl))
{
    Console.WriteLine("ERROR: DATABASE_URL was not found in .env.");
    return;
}

// Convert the PostgreSQL URL into an Npgsql connection string.
var uri = new Uri(databaseUrl);
var credentials = uri.UserInfo.Split(':', 2);

if (credentials.Length != 2)
{
    Console.WriteLine("ERROR: Invalid database credentials format.");
    return;
}

var connectionBuilder = new NpgsqlConnectionStringBuilder
{
    Host = uri.Host,
    Port = uri.IsDefaultPort ? 5432 : uri.Port,
    Database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),
    Username = Uri.UnescapeDataString(credentials[0]),
    Password = Uri.UnescapeDataString(credentials[1]),
    SslMode = SslMode.Require
};

var query = uri.Query.TrimStart('?').Split(
    '&', StringSplitOptions.RemoveEmptyEntries);

foreach (var parameter in query)
{
    var parts = parameter.Split('=', 2);
    if (parts.Length != 2) continue;

    var key = Uri.UnescapeDataString(parts[0]);
    var value = Uri.UnescapeDataString(parts[1]);

    if (key.Equals("sslmode", StringComparison.OrdinalIgnoreCase))
        connectionBuilder.SslMode = value.Equals("disable", StringComparison.OrdinalIgnoreCase)
            ? SslMode.Disable : SslMode.Require;
}

var options = new DbContextOptionsBuilder<SmartStockDbContext>()
    .UseNpgsql(connectionBuilder.ConnectionString)
    .Options;

await using var db = new SmartStockDbContext(options);

try
{
    await db.Database.OpenConnectionAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Database connection failed: {ex.Message}");
    return;
}

Console.WriteLine("Connected to Neon PostgreSQL.");

// Load existing categories and products.

if (args.Contains("--update-categories"))
{
    await CategoryUpdater.RunAsync(db);
    return;
}

var categories = await db.Categories.AsNoTracking().ToListAsync();
var existingSkus = (await db.Products.AsNoTracking()
    .Select(p => p.Sku)
    .ToListAsync())
    .ToHashSet(StringComparer.OrdinalIgnoreCase);

int CategoryId(string name) =>
    categories.First(c =>
        c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).Id;

var kitchenProducts = new List<Product>
{
    new() { Sku="APP-001", Name="Hand Blender", CategoryId=CategoryId("Small Kitchen Appliances"), Price=49.99m, Quantity=12, ReorderLevel=5, ImageUrl="/images/hand-blenders.webp" },
    new() { Sku="APP-002", Name="Countertop Blender", CategoryId=CategoryId("Small Kitchen Appliances"), Price=129.99m, Quantity=3, ReorderLevel=5, ImageUrl="/images/blender.webp" },
    new() { Sku="COO-001", Name="Copper Stock Pot", CategoryId=CategoryId("Cookware and Utensils"), Price=89.99m, Quantity=8, ReorderLevel=5, ImageUrl="/images/copper-pot.webp" },
    new() { Sku="APP-003", Name="Instant Pot", CategoryId=CategoryId("Small Kitchen Appliances"), Price=119.99m, Quantity=6, ReorderLevel=5, ImageUrl="/images/instant-pot.webp" },
    new() { Sku="APP-004", Name="Stand Mixer", CategoryId=CategoryId("Small Kitchen Appliances"), Price=299.99m, Quantity=4, ReorderLevel=5, ImageUrl="/images/stand-mixer.webp" },
    new() { Sku="COO-002", Name="Cast Iron Pan", CategoryId=CategoryId("Cookware and Utensils"), Price=54.99m, Quantity=10, ReorderLevel=5, ImageUrl="/images/cast-iron-pan.webp" },
    new() { Sku="APP-005", Name="Rice Cooker", CategoryId=CategoryId("Small Kitchen Appliances"), Price=79.99m, Quantity=5, ReorderLevel=5, ImageUrl="/images/rice-cooker.webp" },
    new() { Sku="UTE-001", Name="Pastry Cutter", CategoryId=CategoryId("Cookware and Utensils"), Price=14.99m, Quantity=18, ReorderLevel=5, ImageUrl="/images/pastry-cutter.webp" },
    new() { Sku="UTE-002", Name="Vegetable Peelers", CategoryId=CategoryId("Cookware and Utensils"), Price=12.99m, Quantity=2, ReorderLevel=5, ImageUrl="/images/vegetable-peelers.webp" },
    new() { Sku="UTE-003", Name="Wooden Spoon", CategoryId=CategoryId("Cookware and Utensils"), Price=8.99m, Quantity=25, ReorderLevel=5, ImageUrl="/images/wooden-spoon.webp" },
    new() { Sku="UTE-004", Name="Whisks", CategoryId=CategoryId("Cookware and Utensils"), Price=11.99m, Quantity=15, ReorderLevel=5, ImageUrl="/images/whisks.webp" },
    new() { Sku="ACC-001", Name="Silverware Set", CategoryId=CategoryId("Home Essentials and Accessories"), Price=39.99m, Quantity=0, ReorderLevel=5, ImageUrl="/images/silverware.webp" },
    new() { Sku="STO-001", Name="Glass Food Storage Containers", CategoryId=CategoryId("Food Storage and Organization"), Price=29.99m, Quantity=25, ReorderLevel=5, ImageUrl="/images/glass-storage.webp" },
    new() { Sku="STO-002", Name="Kitchen Storage Jars", CategoryId=CategoryId("Food Storage and Organization"), Price=24.99m, Quantity=30, ReorderLevel=5, ImageUrl="/images/storage-jars.webp" },
    new() { Sku="STO-003", Name="White Storage Containers", CategoryId=CategoryId("Food Storage and Organization"), Price=19.99m, Quantity=20, ReorderLevel=5, ImageUrl="/images/white-containers.webp" }
};

var missingProducts = kitchenProducts
    .Where(p => !existingSkus.Contains(p.Sku))
    .ToList();

Console.WriteLine($"\nKitchen products defined: {kitchenProducts.Count}");
Console.WriteLine($"Already in Neon: {kitchenProducts.Count - missingProducts.Count}");
Console.WriteLine($"Ready to import: {missingProducts.Count}\n");

foreach (var product in missingProducts)
{
    Console.WriteLine(
        $"{product.Sku,-10} {product.Name,-32} ${product.Price,7:F2}  Stock: {product.Quantity}");
}

if (missingProducts.Count == 0)
{
    Console.WriteLine("\nAll kitchen products are already in Neon.");
    return;
}

Console.Write("\nType IMPORT to add these products to Neon: ");
var confirmation = Console.ReadLine();

if (confirmation != "IMPORT")
{
    Console.WriteLine("Import cancelled. No products were added.");
    return;
}

await using var transaction = await db.Database.BeginTransactionAsync();

try
{
    db.Products.AddRange(missingProducts);
    await db.SaveChangesAsync();
    await transaction.CommitAsync();

    Console.WriteLine(
        $"\nSUCCESS! Imported {missingProducts.Count} kitchen products into Neon.");
}
catch (Exception ex)
{
    await transaction.RollbackAsync();
    Console.WriteLine($"Import failed: {ex.Message}");
}

