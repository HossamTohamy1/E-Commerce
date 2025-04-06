using System.ComponentModel.DataAnnotations;

namespace E_Commers.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }
        public string UserId { get; set; } // ربط بـ ApplicationUser
        public string Message { get; set; }
        public DateTime NotificationDate { get; set; }
        public bool IsRead { get; set; }
        public ApplicationUser User { get; set; }
    }
}
