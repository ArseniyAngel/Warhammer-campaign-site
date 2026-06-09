namespace CampaignApp.Models
{
    public class Squad
    {
        public int Id { get; set; }
        public string UnitType { get; set; } = "";  
        public string CustomName { get; set; } = "";
        
        public string Type { get; set; } = "Infantry";
        public int PointsCost { get; set; } = 0;     

        
        public int? ScarId { get; set; }

        public int UserId { get; set; }
    }
}