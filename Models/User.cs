namespace CampaignApp.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        
        // Хеш пароля (безопасность согласно академическим стандартам)
        public string PasswordHash { get; set; } = string.Empty;
        
        // Роль: "Admin" (Гейм-мастер) или "Player" (Игрок)
        public string Role { get; set; } = "Player";

        // Внешний ключ для связи с фракцией
        public int? FactionId { get; set; }
        // Навигационное свойство для Entity Framework
        public Faction? Faction { get; set; }

        // Текущий баланс очков фракции у игрока
        public int FactionPointsBalance { get; set; } = 0;
    }
}