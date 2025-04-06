using System.ComponentModel.DataAnnotations;

namespace E_Commers.Models
{
    public class SiteSettings
    {
        [Key]
        public int SettingId { get; set; }
        public string SettingName { get; set; }
        public string SettingValue { get; set; }
    }
}
