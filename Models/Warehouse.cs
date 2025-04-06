namespace E_Commers.Models
{
    public class Warehouse
    {
        public int WarehouseId { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Address { get; set; }
        public decimal Capacity { get; set; }


        // Navigation properties
        public ICollection<Inventory> Inventories { get; set; }
    }
}

