namespace E_Commers.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public string UserId { get; set; }
        public int ShippingAddressId { get; set; }
        public int? BillingAddressId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ShippingCost { get; set; }
        public string OrderStatus { get; set; } = "Pending"; // Pending, Processing, Shipped, Delivered, Cancelled
        public string PaymentMethod { get; set; }
        public string TransactionId { get; set; }

        // Navigation properties
        public ApplicationUser User { get; set; } // إضافة هذه الخاصية

        public Address ShippingAddress { get; set; }
        public Address BillingAddress { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
        public Payment Payment { get; set; }
        public ShippingDetail ShippingDetail { get; set; }
    }
}
