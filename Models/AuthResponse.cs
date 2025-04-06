namespace E_Commers.Models
{
    public class AuthResponse
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }

    }
}
