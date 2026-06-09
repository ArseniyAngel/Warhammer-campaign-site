using System;

namespace CampaignApp.Models
{
    // Этот класс описывает структуру таблицы в базе данных PostgreSQL
    public class NewsPost
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}