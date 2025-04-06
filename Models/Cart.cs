using E_Commers.Models;

public class Cart
{
    public int CartId { get; set; }
    public string UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ApplicationUser User { get; set; }
    public ICollection<CartItem> Items { get; set; }
}