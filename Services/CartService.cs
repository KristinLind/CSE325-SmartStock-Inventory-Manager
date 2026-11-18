using SmartStock.Models;

namespace SmartStock.Services;

public class CartService
{
    private readonly List<CartItem> _items = new();

    public IReadOnlyList<CartItem> Items => _items;
    public int ItemCount => _items.Sum(i => i.Quantity);
    public decimal Total => _items.Sum(i => i.LineTotal);

    // Lets the menu badge and cart page refresh when the cart changes
    public event Action? OnChange;

    public string? Add(Product product, int quantity = 1)
    {
        if (quantity < 1) return "Quantity must be at least 1.";
        if (product.Quantity <= 0) return $"'{product.Name}' is out of stock.";

        var existing = _items.FirstOrDefault(i => i.ProductId == product.Id);
        var newQuantity = (existing?.Quantity ?? 0) + quantity;

        if (newQuantity > product.Quantity)
            return $"Only {product.Quantity} of '{product.Name}' in stock.";

        if (existing is null)
        {
            _items.Add(new CartItem
            {
                ProductId = product.Id,
                Name = product.Name,
                Sku = product.Sku,
                UnitPrice = product.Price,
                Quantity = quantity,
                InStock = product.Quantity
            });
        }
        else
        {
            existing.Quantity = newQuantity;
            existing.InStock = product.Quantity;
            existing.UnitPrice = product.Price;
        }

        OnChange?.Invoke();
        return null;
    }

    public string? SetQuantity(int productId, int quantity)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null) return null;

        if (quantity <= 0)
        {
            Remove(productId);
            return null;
        }

        if (quantity > item.InStock)
            return $"Only {item.InStock} of '{item.Name}' in stock.";

        item.Quantity = quantity;
        OnChange?.Invoke();
        return null;
    }

    public void Remove(int productId)
    {
        _items.RemoveAll(i => i.ProductId == productId);
        OnChange?.Invoke();
    }

    public void Clear()
    {
        _items.Clear();
        OnChange?.Invoke();
    }
}