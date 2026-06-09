using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace CampaignApp.Models
{
    public class Sector
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = "";
        

        public int? ControllingFactionId { get; set; }
        

        [JsonIgnore] 
        public Faction? ControllingFaction { get; set; }

        public string ResourceType { get; set; } = "";
        public string Coordinates { get; set; } = "";


        public string Description { get; set; } = "Особых примет ландшафта не зарегистрировано.";
        public string GMMarks { get; set; } = "";
        public bool IsUnderground { get; set; } = false;
        public string MissionName { get; set; } = "Разведывательная миссия";
        public string MissionStatus { get; set; } = "active";
        public string VoterListJson { get; set; } = "[]";
    

    public string FilesJson { get; set; } = "[]";
    [NotMapped] /
    [JsonPropertyName("files")] 
    public List<CampaignApp.Controllers.MapController.MissionFileDto> Files
    {
        get
        {
            if (string.IsNullOrWhiteSpace(FilesJson)) 
                return new List<CampaignApp.Controllers.MapController.MissionFileDto>();
            try
            {
                return JsonSerializer.Deserialize<List<CampaignApp.Controllers.MapController.MissionFileDto>>(FilesJson) 
                    ?? new List<CampaignApp.Controllers.MapController.MissionFileDto>();
            }
            catch
            {
                return new List<CampaignApp.Controllers.MapController.MissionFileDto>();
            }
        }
    }
    }
}