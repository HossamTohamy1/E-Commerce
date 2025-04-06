using Microsoft.AspNetCore.Identity;

namespace E_Commers.Models
{
    public class ApplicationRole : IdentityRole
    {
        public string Description { get; set; }
    }
}
