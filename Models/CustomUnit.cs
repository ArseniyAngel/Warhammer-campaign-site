namespace CampaignApp.Models
{
    public class CustomUnit
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Название отряда
        public string Type { get; set; } = string.Empty; // Infantry, Vehicle, Monster, Character
        
        public int PointsValue { get; set; } // Стоимость в pts (например, 150)

        // Статус отряда: "Active" или "Out of Action" (Выведен из строя)
        public string Status { get; set; } = "Active"; 

        // Описание шрамов войны или улучшений, полученных за ОФ
        public string BattleScarsAndUpgrades { get; set; } = string.Empty;

        // Связь с владельцем-игроком
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}