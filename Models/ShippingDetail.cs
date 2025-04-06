namespace E_Commers.Models
{
    public class ShippingDetail
    {
        public int ShippingDetailId { get; set; }
        public int OrderId { get; set; }
        public string ShippingMethod { get; set; }
        public string TrackingNumber { get; set; }
        public DateTime? ShippingDate { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public string ShippingStatus { get; set; } = "Pending"; // Pending, Shipped, Delivered

        // Navigation property
        public Order Order { get; set; }
    }
}
