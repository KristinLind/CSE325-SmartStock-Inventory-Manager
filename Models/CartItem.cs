namespace SmartStock.Models;

public class CartItem
{
    public int ProductId { get; set; }
    public string Name { get; set; } = "";
    public string Sku { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }

    // Stock available when the item was added, used to cap the quantity
    public int InStock { get; set; }

    public decimal LineTotal => UnitPrice * Quantity;
}