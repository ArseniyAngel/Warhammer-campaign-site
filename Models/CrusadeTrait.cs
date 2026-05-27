namespace CampaignApp.Models
{
    public class CrusadeTrait
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Type { get; set; } = ""; // "Scar" или "Upgrade"
        public string UnitTypeRestriction { get; set; } = "All"; // "All", "Infantry", "Vehicle", "Monster"
        
        // --- НОВЫЕ ПОЛЯ ---
        public int PtsModifier { get; set; } = 0;
        public int FractionPointsCost { get; set; } = 0;
        public string FactionName { get; set; } = "All";
    }
}