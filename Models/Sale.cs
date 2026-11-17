namespace SmartStock.Models;

public class Sale
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int QuantitySold { get; set; }

    // Price at the time of sale, so old sales don't change if the price changes later
    public decimal UnitPrice { get; set; }

    public DateTime SoldAt { get; set; } = DateTime.Now;

    public decimal Total => UnitPrice * QuantitySold;
}