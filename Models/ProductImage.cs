namespace E_Commers.Models
{
    public class ProductImage
    {
        public int ProductImageId { get; set; }
        public int ProductId { get; set; }
        public string ImageUrl { get; set; }
        public bool IsPrimary { get; set; }
        public int DisplayOrder { get; set; }

        // Navigation property
        public Product Product { get; set; }
    }
}
