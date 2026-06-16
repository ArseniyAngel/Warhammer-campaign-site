using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CampaignApp.Data;
using CampaignApp.Models;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CampaignApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MapController : ControllerBase
    {
        private readonly CampaignContext _context;

        public MapController(CampaignContext context)
        {
            _context = context;
        }

        // 1. МЕТОД ПОЛУЧЕНИЯ КАРТЫ
        [HttpGet]
        public ActionResult<IEnumerable<object>> GetMap()
        {
            var dbSectors = _context.Sectors.ToList();
            var username = User.Identity?.Name;
            bool isAdmin = User.Identity.IsAuthenticated && User.IsInRole("Admin");

            var response = dbSectors.Select(s => {
                List<string> voters = new List<string>();
                try { 
                    voters = JsonSerializer.Deserialize<List<string>>(s.VoterListJson ?? "[]") ?? new List<string>(); 
                } catch { }

                return new {
                    s.Id,
                    s.Name,
                    s.ControllingFactionId,
                    s.Description,
                    s.MissionName,
                    s.MissionStatus,
                    s.Files,
                    s.Coordinates,
                    s.GMMarks, 
                    VoterList = isAdmin ? voters : null,
                    HasVoted = username != null && voters.Contains(username)
                };
            });

            return Ok(response);
        }

        public class MissionFileDto
        {
            [JsonPropertyName("name")] public string Name { get; set; }
            [JsonPropertyName("url")] public string Url { get; set; }
        }

        public class SectorUpdateDto
        {
            [JsonPropertyName("name")] public string Name { get; set; }
            [JsonPropertyName("controllingFactionId")] public int ControllingFactionId { get; set; }
            [JsonPropertyName("description")] public string Description { get; set; } = "";
            [JsonPropertyName("gmMarks")] public string GMMarks { get; set; } = "";
            [JsonPropertyName("missionName")] public string MissionName { get; set; } = "Разведывательная миссия";
            [JsonPropertyName("missionStatus")] public string MissionStatus { get; set; } = "active";
            [JsonPropertyName("files")] public List<MissionFileDto> Files { get; set; } = new List<MissionFileDto>();
        }

        // 2. МЕТОД ОБНОВЛЕНИЯ СЕКТОРА
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult UpdateSector(int id, [FromBody] SectorUpdateDto dto)
        {
            var sector = _context.Sectors.Find(id);
            if (sector == null) return NotFound("Сектор не найден.");

            sector.Name = !string.IsNullOrWhiteSpace(dto.Name) ? dto.Name.Trim() : sector.Name;
            sector.ControllingFactionId = dto.ControllingFactionId == 0 ? (int?)null : dto.ControllingFactionId;
            sector.Description = dto.Description ?? "";
            sector.GMMarks = dto.GMMarks ?? "";
            sector.MissionName = dto.MissionName ?? "Разведывательная миссия";
            sector.MissionStatus = dto.MissionStatus ?? "active";

            sector.FilesJson = dto.Files != null ? JsonSerializer.Serialize(dto.Files) : "[]";

            _context.SaveChanges();
            return Ok(sector);
        }

        // 3. МЕТОД ГОЛОСОВАНИЯ
        [HttpPost("{id}/vote")]
        [Authorize(Roles = "Player,Admin")]
        public IActionResult VoteForSector(int id)
        {
            var sector = _context.Sectors.Find(id);
            if (sector == null) return NotFound("Сектор не найден.");

            var username = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(username)) return Unauthorized();

            List<string> voters;
            try 
            {
                string json = sector.VoterListJson;
                if (string.IsNullOrWhiteSpace(json)) 
                {
                    voters = new List<string>();
                }
                else 
                {
                    voters = JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
                }
            }
            catch 
            {
                voters = new List<string>();
            }

            if (!voters.Contains(username))
            {
                voters.Add(username);
                sector.VoterListJson = JsonSerializer.Serialize(voters);
                _context.SaveChanges();
            }

            return Ok(sector);
        }
    }
}