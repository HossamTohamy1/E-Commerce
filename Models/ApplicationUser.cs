using Microsoft.AspNetCore.Identity;

namespace E_Commers.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsBlocked { get; set; }
        public int FailedAttempts { get; set; } = 0; // عدد المحاولات الفاشلة
        public bool IsLocked { get; set; } = false; // حالة القفل
        public DateTime? LockoutEnd { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual ICollection<Address> Addresses { get; set; }
        public virtual ICollection<Cart> Carts { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<Wishlist> Wishlists { get; set; }
        public virtual ICollection<ProductSeller> ProductSellers { get; set; }
        public ICollection<SellerUpgradeRequest> SellerUpgradeRequests { get; set; }
        public ICollection<Store> Stores { get; set; }
        public ICollection<PromotionRequest> PromotionRequests { get; set; }
        public ICollection<Notification> Notifications { get; set; }
        public ICollection<Complaint> Complaints { get; set; }

    }
}
