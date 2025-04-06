namespace E_Commers.Models
{
    public class ProductCategory
    {
        public int ProductId { get; set; }
        public int CategoryId { get; set; }

        // Navigation properties
        public Product Product { get; set; }
        public Category Category { get; set; }
    }
}
