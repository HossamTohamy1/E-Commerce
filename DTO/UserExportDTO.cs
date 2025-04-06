namespace E_Commers.DTO
{
    public class UserExportDTO
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public List<OrderExportDTO> Orders { get; set; }
        public List<ComplaintDTO> Complaints { get; set; }
    }
}
