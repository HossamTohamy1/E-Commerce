namespace E_Commers.Models
{
    public class ProductAttribute
    {
        public int ProductAttributeId { get; set; }
        public int ProductId { get; set; }
        public string AttributeName { get; set; }
        public string AttributeValue { get; set; }

        // Navigation property
        public Product Product { get; set; }
    }
}
