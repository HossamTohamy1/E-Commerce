namespace E_Commers.Models
{
    public class Recommendation
    {
        public int RecommendationId { get; set; }
        public string UserId { get; set; }
        public int ProductId { get; set; }
        public decimal Score { get; set; }
        public string RecommendationType { get; set; } // BasedOnHistory, SimilarUsers, etc.
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ApplicationUser User { get; set; } // إضافة هذه الخاصية

        public Product Product { get; set; }
    }
}
