namespace E_Commers.Models
{
    public class ProductSeller
    {
        public int ProductId { get; set; }
        public string UserId { get; set; }
        public decimal SellerPrice { get; set; }
        public int StockInSeller { get; set; }

        // Navigation properties
        public Product Product { get; set; }
        public ApplicationUser User { get; set; }
    }
}
