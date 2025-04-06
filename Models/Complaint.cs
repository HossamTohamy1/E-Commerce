using System.ComponentModel.DataAnnotations;

namespace E_Commers.Models
{
    public class Complaint
    {
        [Key]
        public int ComplaintId { get; set; }
        public string UserId { get; set; } // ربط بـ ApplicationUser
        public string Subject { get; set; }
        public string Description { get; set; }
        public DateTime ComplaintDate { get; set; }
        public string Status { get; set; } // مثل "Open", "Closed"
        public ApplicationUser User { get; set; }
    }
}
