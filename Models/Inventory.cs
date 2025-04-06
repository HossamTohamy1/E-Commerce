namespace E_Commers.Models
{
    public class Inventory
    {
        public int InventoryId { get; set; }
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public int Quantity { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Product Product { get; set; }
        public Warehouse Warehouse { get; set; }
    }
}
