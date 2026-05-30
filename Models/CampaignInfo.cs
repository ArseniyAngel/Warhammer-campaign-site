using System.ComponentModel.DataAnnotations;

namespace CampaignApp.Models
{
    public class CampaignInfo
    {
        [Key]
        public string Key { get; set; } // Оставим "regulations"
        [Required]
        public string Content { get; set; } // Текст регламента
        public string FileUrl { get; set; } // Ссылка на прикрепленный PDF/документ rules
    }
}