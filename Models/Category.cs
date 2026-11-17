using System.ComponentModel.DataAnnotations;

namespace SmartStock.Models;

public class Category
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Category name is required")]
    [StringLength(60)]
    public string Name { get; set; } = "";

    public List<Product> Products { get; set; } = new();
}