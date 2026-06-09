namespace CampaignApp.Models
{
    public class CustomUnit
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; 
        public string Type { get; set; } = string.Empty;
        
        public int PointsValue { get; set; }

        public string Status { get; set; } = "Active"; 

        public string BattleScarsAndUpgrades { get; set; } = string.Empty;

        public int UserId { get; set; }
        public User? User { get; set; }
    }
}