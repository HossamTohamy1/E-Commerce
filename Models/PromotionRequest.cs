using System.ComponentModel.DataAnnotations;

namespace E_Commers.Models
{
    public class PromotionRequest
    {
        [Key]
        public int RequestId { get; set; }
        public int PromotionId { get; set; }
        public string UserId { get; set; } // ربط بـ ApplicationUser
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } // مثل "Pending", "Approved", "Rejected"
        public Promotion Promotion { get; set; }
        public ApplicationUser User { get; set; }
    }
}
