using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CampaignApp.Data;
using CampaignApp.Models;
using System.Linq;
using System.Text.Json.Serialization;

namespace CampaignApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class SquadsController : ControllerBase
    {
        private readonly CampaignContext _context;

        public SquadsController(CampaignContext context)
        {
            _context = context;
        }

        public class GMModData
        {
            [JsonPropertyName("pointsCost")]
            public int PointsCost { get; set; }

            [JsonPropertyName("scarId")]
            public int ScarId { get; set; }
        }

        public class BuyUpgradeRequest
        {
            [JsonPropertyName("squadId")]
            public int SquadId { get; set; }

            [JsonPropertyName("traitId")]
            public int TraitId { get; set; }
        }

        // ==========================================
        // 1. МЕТОДЫ ДЛЯ ИГРОКОВ (ЛИЧНЫЙ ШТАБ)
        // ==========================================

        [HttpGet("my-squads")]
        public IActionResult GetMySquads()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (userIdClaim == null) return Unauthorized();
            int userId = int.Parse(userIdClaim);

            var user = _context.Users.Include(u => u.Faction).FirstOrDefault(u => u.Id == userId);
            if (user == null) return NotFound();

            var squads = _context.Squads.Where(s => s.UserId == userId).ToList();
            var allTraits = _context.CrusadeTraits.ToList();
            var squadUpgrades = _context.SquadUpgrades.ToList();

            var resultSquads = squads.Select(s => {
                var dynamicTraitIds = squadUpgrades
                    .Where(su => su.SquadId == s.Id)
                    .Select(su => su.CrusadeTraitId)
                    .ToList();

                var currentTraits = allTraits.Where(t => dynamicTraitIds.Contains(t.Id)).ToList();

                int finalPtsCost = s.PointsCost + currentTraits.Where(t => t.Type == "Upgrade").Sum(t => t.PtsModifier);

                return new {
                    id = s.Id,
                    unitType = s.UnitType ?? "Infantry", 
                    customName = s.CustomName ?? "Без имени",
                    type = s.Type ?? "",
                    basePointsCost = s.PointsCost,
                    pointsCost = finalPtsCost,
                    upgrades = currentTraits.Where(t => t.Type == "Upgrade").Select(t => new { id = t.Id, name = t.Name, ptsModifier = t.PtsModifier, description = t.Description }).ToList(),
                    scars = currentTraits.Where(t => t.Type == "Scar").Select(t => new { id = t.Id, name = t.Name, description = t.Description }).ToList()
                };
            }).ToList();

            string fName = user.Faction?.Name ?? "Неизвестная Фракция";
            string pName = user.Faction?.ResourceName ?? "Очки Фракции";

            // Проверяем роль напрямую из БД для активации ГМ-панели на фронтенде
            bool isGM = (user.Role == "Admin");

            return Ok(new { 
                squads = resultSquads, 
                pointsBalance = user.FactionPointsBalance, 
                pointsName = pName,
                factionName = fName,
                isGMSession = isGM 
            });
        }

        [HttpPost("add")]
        public IActionResult AddSquad([FromBody] Squad newSquad)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (userIdClaim == null) return Unauthorized();

            newSquad.UserId = int.Parse(userIdClaim);
            newSquad.ScarId = null;

            _context.Squads.Add(newSquad);
            _context.SaveChanges();
            return Ok(newSquad);
        }

        [HttpPost("buy-upgrade")]
        public IActionResult BuyFactionUpgrade([FromBody] BuyUpgradeRequest request)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (userIdClaim == null) return Unauthorized();
            int userId = int.Parse(userIdClaim);

            var user = _context.Users.Include(u => u.Faction).FirstOrDefault(u => u.Id == userId);
            if (user == null) return NotFound("Пользователь не найден.");

            var squad = _context.Squads.FirstOrDefault(s => s.Id == request.SquadId && s.UserId == userId);
            if (squad == null) return NotFound("Отряд не найден или не принадлежит вам.");

            var trait = _context.CrusadeTraits.Find(request.TraitId);
            if (trait == null || trait.Type != "Upgrade") return BadRequest("Улучшение не найдено.");

            var traitFactionProp = trait.GetType().GetProperty("FactionName") ?? trait.GetType().GetProperty("factionName");
            string traitFaction = traitFactionProp != null ? traitFactionProp.GetValue(trait)?.ToString() : "All";

            if (traitFaction != "All" && traitFaction != user.Faction?.Name)
            {
                return BadRequest($"Это улучшение для фракции {traitFaction}, а ваша фракция — {user.Faction?.Name}.");
            }

            // ИСПРАВЛЕНО: Проверяем ограничение (UnitTypeRestriction) по полю архетипа (squad.Type), а не по имени модели (squad.UnitType)
            if (trait.UnitTypeRestriction != "All" && !trait.UnitTypeRestriction.Contains(squad.Type))
            {
                return BadRequest($"Данное улучшение нельзя применить к типу '{squad.Type}'. Требуется категория: {trait.UnitTypeRestriction}");
            }

            if (user.FactionPointsBalance < trait.FractionPointsCost)
            {
                return BadRequest($"Недостаточно ресурсов. Требуется: {trait.FractionPointsCost}");
            }

            var alreadyBought = _context.SquadUpgrades.Any(su => su.SquadId == request.SquadId && su.CrusadeTraitId == request.TraitId);
            if (alreadyBought) return BadRequest("Этот отряд уже приобрел данную модернизацию.");

            user.FactionPointsBalance -= trait.FractionPointsCost;
            _context.SquadUpgrades.Add(new SquadUpgrade { SquadId = squad.Id, CrusadeTraitId = trait.Id });

            _context.SaveChanges();
            return Ok(new { newBalance = user.FactionPointsBalance });
        }

        [HttpPost("set-faction")]
        public IActionResult SetFaction([FromBody] int factionId)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (userIdClaim == null) return Unauthorized();
            int userId = int.Parse(userIdClaim);

            var user = _context.Users.Find(userId);
            if (user == null) return NotFound();

            user.FactionId = factionId;
            _context.SaveChanges();
            return Ok();
        }

        [HttpGet("traits-list")]
        public IActionResult GetTraitsList()
        {
            var traits = _context.CrusadeTraits.ToList().Select(t => {
                var fProp = t.GetType().GetProperty("FactionName") ?? t.GetType().GetProperty("factionName");
                string fName = fProp != null ? fProp.GetValue(t)?.ToString() : "All";
                
                // ИСПРАВЛЕНИЕ: Если в БД записана пустая строка или null, принудительно ставим "All"
                if (string.IsNullOrWhiteSpace(fName)) 
                {
                    fName = "All";
                }

                return new {
                    id = t.Id,
                    name = t.Name,
                    description = t.Description,
                    type = t.Type,
                    unitTypeRestriction = t.UnitTypeRestriction,
                    ptsModifier = t.PtsModifier,
                    fractionPointsCost = t.FractionPointsCost,
                    factionName = fName // Теперь тут гарантированно либо имя фракции, либо "All"
                };
            }).ToList();

            return Ok(traits);
        }

        // ==========================================
        // 2. МЕТОДЫ ГЕЙМ-МАСТЕРА (ADMIN)
        // ==========================================

        [HttpGet("admin/users")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAllUsersForAdmin()
        {
            var users = _context.Users.Select(u => new { id = u.Id, username = u.Username, role = u.Role, factionPointsBalance = u.FactionPointsBalance }).ToList();
            return Ok(users);
        }

        [HttpGet("admin/user-squads/{userId}")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetUserSquadsForAdmin(int userId)
        {
            var squads = _context.Squads.Where(s => s.UserId == userId).ToList();
            var allTraits = _context.CrusadeTraits.ToList();
            var squadUpgrades = _context.SquadUpgrades.ToList();

            var result = squads.Select(s => {
                var attachedTraitIds = squadUpgrades
                    .Where(su => su.SquadId == s.Id)
                    .Select(su => su.CrusadeTraitId)
                    .ToList();

                var currentTraits = allTraits.Where(t => attachedTraitIds.Contains(t.Id)).ToList();

                return new {
                    id = s.Id,
                    customName = s.CustomName ?? "Без имени",
                    unitType = s.UnitType ?? "Infantry",
                    type = s.Type ?? "",
                    pointsCost = s.PointsCost, 
                    upgrades = currentTraits.Where(t => t.Type == "Upgrade").Select(t => new { id = t.Id, name = t.Name, ptsModifier = t.PtsModifier }).ToList(),
                    scars = currentTraits.Where(t => t.Type == "Scar").Select(t => new { id = t.Id, name = t.Name, description = t.Description }).ToList()
                };
            }).ToList();

            return Ok(result);
        }

        [HttpPut("admin/mod-squad/{squadId}")]
        [Authorize(Roles = "Admin")]
        public IActionResult ModSquadByGM(int squadId, [FromBody] GMModData data)
        {
            if (data == null) return BadRequest("Данные не получены.");

            var squad = _context.Squads.Find(squadId);
            if (squad == null) return NotFound("Отряд не найден.");

            squad.PointsCost = data.PointsCost;

            if (data.ScarId > 0)
            {
                var alreadyHasScar = _context.SquadUpgrades.Any(su => su.SquadId == squadId && su.CrusadeTraitId == data.ScarId);
                if (!alreadyHasScar)
                {
                    _context.SquadUpgrades.Add(new SquadUpgrade { 
                        SquadId = squadId, 
                        CrusadeTraitId = data.ScarId 
                    });
                }
            }

            _context.SaveChanges();
            return Ok();
        }

        [HttpDelete("admin/remove-trait/{squadId}/{traitId}")]
        [Authorize(Roles = "Admin")]
        public IActionResult RemoveTraitFromSquad(int squadId, int traitId)
        {
            var link = _context.SquadUpgrades
                .FirstOrDefault(su => su.SquadId == squadId && su.CrusadeTraitId == traitId);

            if (link == null) return NotFound("Данная модификация не найдена.");

            _context.SquadUpgrades.Remove(link);
            _context.SaveChanges();

            return Ok();
        }
        [HttpPost("admin/add-trait")]
        [Authorize(Roles = "Admin")] // Сюда пустит только Гейм-Мастера
        public IActionResult AddNewTrait([FromBody] CrusadeTraitDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Description))
            {
                return BadRequest("Название и описание улучшения не могут быть пустыми!");
            }

            // Создаем новый объект для БД
            var newTrait = new CrusadeTrait
            {
                Name = dto.Name,
                Description = dto.Description,
                Type = "Upgrade", // Устанавливаем тип как Модернизация
                UnitTypeRestriction = dto.UnitTypeRestriction,
                PtsModifier = dto.PtsModifier,
                FractionPointsCost = dto.FractionPointsCost,
                // Если ГМ выбрал общую прокачку, пишем "All", иначе — имя конкретной фракции
                FactionName = dto.IsGeneral ? "All" : dto.FactionName 
            };

            _context.CrusadeTraits.Add(newTrait);
            _context.SaveChanges();

            return Ok(new { message = "Модернизация успешно добавлена в Кодекс!", traitId = newTrait.Id });
        }

        // Вспомогательный класс (DTO) для приема данных от фронтенда
        public class CrusadeTraitDto
        {
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
            public string UnitTypeRestriction { get; set; } = "All";
            public int PtsModifier { get; set; }
            public int FractionPointsCost { get; set; }
            public bool IsGeneral { get; set; }
            public string FactionName { get; set; } = "";
        }
        [HttpPut("admin/add-user-points/{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult AddUserPoints(int id, [FromBody] int pointsDelta)
        {
            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            user.FactionPointsBalance += pointsDelta;
            if (user.FactionPointsBalance < 0) user.FactionPointsBalance = 0;

            _context.SaveChanges();
            return Ok(new { newBalance = user.FactionPointsBalance });
        }
    }
}