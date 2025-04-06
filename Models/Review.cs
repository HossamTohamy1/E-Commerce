namespace E_Commers.Models
{
    public class Review
    {
        public int ReviewId { get; set; }
        public int ProductId { get; set; }
        public string UserId { get; set; }
        public int Rating { get; set; } // 1-5
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsApproved { get; set; } = false;

        // Navigation properties
        public Product Product { get; set; }
        public ApplicationUser User { get; set; } // إضافة هذه الخاصية
    }
}
