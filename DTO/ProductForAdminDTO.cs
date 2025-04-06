namespace E_Commers.DTO
{
    public class ProductForAdminDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal? Price { get; set; }
        public int? StockQuantity { get; set; }
        public int? CategoryId { get; set; }
        public string ProductImage { get; set; } 

        public string SKU { get; set; }
    }
}
