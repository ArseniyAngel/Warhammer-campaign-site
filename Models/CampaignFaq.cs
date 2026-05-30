using System.ComponentModel.DataAnnotations;

namespace CampaignApp.Models
{
    public class CampaignFaq
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Question { get; set; } // Вопрос
        [Required]
        public string Answer { get; set; } // Ответ
    }
}