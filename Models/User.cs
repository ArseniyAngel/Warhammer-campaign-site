namespace CampaignApp.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        
        public string PasswordHash { get; set; } = string.Empty;
        
        public string Role { get; set; } = "Player";

        public int? FactionId { get; set; }

        public Faction? Faction { get; set; }

        public int FactionPointsBalance { get; set; } = 0;
    }
}