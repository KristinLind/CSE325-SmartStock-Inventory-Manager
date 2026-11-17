using System.ComponentModel.DataAnnotations;

namespace SmartStock.Models;

public class Product
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [StringLength(100)]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "SKU is required")]
    [StringLength(30)]
    public string Sku { get; set; } = "";

    [Range(1, int.MaxValue, ErrorMessage = "Choose a category")]
    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    [Range(0.01, 1000000, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [Range(0, 1000000, ErrorMessage = "Quantity cannot be negative")]
    public int Quantity { get; set; }

    [Range(0, 1000000, ErrorMessage = "Reorder level cannot be negative")]
    public int ReorderLevel { get; set; } = 5;

    // Calculated values (not stored in the database)
    public bool IsLowStock => Quantity <= ReorderLevel;

    public string StockStatus =>
        Quantity == 0 ? "Out of stock" :
        Quantity <= ReorderLevel ? "Low stock" :
        "In stock";
}