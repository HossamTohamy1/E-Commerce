namespace E_Commers.Models
{
    public class WishlistItem
    {
        public int WishlistItemId { get; set; }
        public int WishlistId { get; set; }
        public int ProductId { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Wishlist Wishlist { get; set; }
        public Product Product { get; set; }
    }
}
