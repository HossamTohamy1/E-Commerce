namespace E_Commers.Models
{
    public class CartItem
    {
        public int CartItemId { get; set; }
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Cart Cart { get; set; }
        public Product Product { get; set; }
    }
}
