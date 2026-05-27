namespace CampaignApp.Models
{
    public class Squad
    {
        public int Id { get; set; }
        public string UnitType { get; set; } = "";   // Что за юнит (напр., "Intercessors")
        public string CustomName { get; set; } = ""; // Кастомное имя отряда
        
        // --- НОВЫЕ ПОЛЯ ---
        public string Type { get; set; } = "Infantry"; // "Infantry", "Vehicle", "Monster"
        public int PointsCost { get; set; } = 0;       // Стоимость отряда в очках (pts)

        // Связи с улучшениями и шрамами (могут быть null, если отряд "чистый")
        public int? ScarId { get; set; }

        public int UserId { get; set; }
    }
}