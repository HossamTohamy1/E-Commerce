namespace E_Commers.DTO
{
    public class SalesReportDTO
    {
        public int TotalOrders { get; set; }
        public int TotalProductsSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public int UniqueBuyers { get; set; }
        public List<TopProductDTO> TopProducts { get; set; } = new();
    }
}
