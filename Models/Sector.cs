using System.ComponentModel.DataAnnotations; // ДОБАВИЛИ ЭТУ СТРОЧКУ
using System.Text.Json.Serialization;
using System.Collections.Generic; // Для работы с List
using System.ComponentModel.DataAnnotations.Schema; // Для [NotMapped]
using System.Text.Json;

namespace CampaignApp.Models
{
    public class Sector
    {
        public int Id { get; set; }
        
        [Required] // Теперь компилятор поймет, что это обязательное поле
        public string Name { get; set; } = "";
        
        // ID фракции (nullable, может быть пустым — это супер)
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
        public string MissionName { get; set; } = "Разведывательная миссия";
        public string MissionStatus { get; set; } = "active";
        public string VoterListJson { get; set; } = "[]";
    
    // Файлы храним как JSON строку внутри текстового поля базы данных
    public string FilesJson { get; set; } = "[]";
    [NotMapped] // Говорит EF Core: "Не создавай такую колонку в БД, это только для кода"
    [JsonPropertyName("files")] // Сериализатор превратит это свойство в массив "files" в JSON
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