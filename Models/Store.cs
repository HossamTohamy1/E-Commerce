using System.ComponentModel.DataAnnotations;

namespace E_Commers.Models
{
    public class Store
    {
        [Key]
        public int StoreId { get; set; }
        public string StoreName { get; set; }
        public string Description { get; set; }
        public string UserId { get; set; } // ربط بـ ApplicationUser
        public ApplicationUser User { get; set; }
    }
}
