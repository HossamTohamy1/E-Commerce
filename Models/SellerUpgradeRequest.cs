namespace E_Commers.Models
{
    public class SellerUpgradeRequest
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string BusinessName { get; set; }
        public string ContactInfo { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; }
        public ApplicationUser User { get; set; }

    }
}
