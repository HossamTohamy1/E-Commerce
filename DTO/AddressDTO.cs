namespace E_Commers.DTO
{
    public class AddressDTO
    {
        public string AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string City { get; set; }
        public string? State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public bool IsDefault { get; set; } = false;
        public string? AddressType { get; set; } 
    }
}
