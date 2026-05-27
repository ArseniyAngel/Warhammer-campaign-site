using System.Text.Json.Serialization;

namespace CampaignApp.Models
{
    public class Sector
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        
        // ID фракции
        public int? ControllingFactionId { get; set; }
        
        // НАВИГАЦИОННОЕ СВОЙСТВО
        [JsonIgnore] 
        public Faction? ControllingFaction { get; set; }

        public string ResourceType { get; set; } = "";
        public string Coordinates { get; set; } = "";

        // НОВЫЕ ПОЛЯ ДЛЯ ПОДЗЕМКИ И МЕТОК ГМ
        public string Description { get; set; } = "Особых примет ландшафта не зарегистрировано.";
        public string GMMarks { get; set; } = "";
        public bool IsUnderground { get; set; } = false;
    }
}