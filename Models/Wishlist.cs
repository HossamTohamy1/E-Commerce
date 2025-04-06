namespace E_Commers.Models
{
    public class Wishlist
    {
        public int WishlistId { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ApplicationUser User { get; set; } // إضافة هذه الخاصية

        public ICollection<WishlistItem> Items { get; set; }
    }
}
