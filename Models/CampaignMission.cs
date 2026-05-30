using System.ComponentModel.DataAnnotations;

namespace CampaignApp.Models
{
    public class CampaignMission
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; } // Название миссии
        [Required]
        public string Description { get; set; } // Текст / Задачи
        public string Reward { get; set; } // Награда фракции
        public bool IsActive { get; set; } = true; // Активна или заблокирована
    }
}