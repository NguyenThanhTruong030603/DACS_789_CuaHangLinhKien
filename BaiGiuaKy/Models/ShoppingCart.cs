public class ShoppingCart
{
	public List<CartItem> Items { get; set; } = new List<CartItem>();
	
	public decimal DiscountPercentage { get; set; }

    public string DiscountCode { get; set; }
    public void AddItem(CartItem item)
	{
		var existingItem = Items.FirstOrDefault(i => i.ProductId ==
		item.ProductId);
		if (existingItem != null)
		{
			existingItem.Quantity += item.Quantity;
		}
		else
		{
			Items.Add(item);
		}
	}
	public void RemoveItem(int productId)
	{
		Items.RemoveAll(i => i.ProductId == productId);
	}
    // Các phương thức khác...
    public int GetTotalQuantity()
    {
        return Items.Sum(i => i.Quantity);
    }
}