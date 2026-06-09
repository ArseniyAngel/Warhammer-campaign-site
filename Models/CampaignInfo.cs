using System.ComponentModel.DataAnnotations;

namespace CampaignApp.Models
{
    public class CampaignInfo
    {
        [Key]
        public string Key { get; set; }
        [Required]
        public string Content { get; set; }
        public string FileUrl { get; set; }
    }
}