using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CampaignApp.Data;
using CampaignApp.Models;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CampaignApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MapController : ControllerBase
    {
        private readonly CampaignContext _context;

        // Внедряем контекст базы данных через конструктор
        public MapController(CampaignContext context)
        {
            _context = context;
        }

        // 1. МЕТОД ПОЛУЧЕНИЯ КАРТЫ: Теперь берёт абсолютно ВСЕ сектора напрямую из БД
        [HttpGet]
        public ActionResult<IEnumerable<Sector>> GetMap()
        {
            var dbSectors = _context.Sectors.ToList();
            return Ok(dbSectors);
        }

        // Вспомогательный класс (DTO) для безопасного приёма изменений из ГМ-панели
        public class SectorUpdateDto
        {
            [JsonPropertyName("controllingFactionId")]
            public int ControllingFactionId { get; set; }

            [JsonPropertyName("description")]
            public string Description { get; set; } = "";

            [JsonPropertyName("gmMarks")]
            public string GMMarks { get; set; } = "";
        }

        // 2. МЕТОД ОБНОВЛЕНИЯ СЕКТОРА ГМ-ОМ
        [HttpPut("{id}")]
[Authorize(Roles = "Admin")]
public IActionResult UpdateSector(int id, [FromBody] SectorUpdateDto dto)
{
    // Ищем сектор в БД
    var sector = _context.Sectors.Find(id);
    if (sector == null) return NotFound("Сектор не найден в тактическом логе.");

    // ИСПРАВЛЕНИЕ: Если ГМ выбрал "Нейтральная земля" (0), записываем null
    if (dto.ControllingFactionId == 0)
    {
        sector.ControllingFactionId = null;
    }
    else
    {
        sector.ControllingFactionId = dto.ControllingFactionId;
    }

    // Обновляем остальные текстовые поля ГМ
    sector.Description = dto.Description;
    sector.GMMarks = dto.GMMarks;

    // Теперь SQLite сохранит изменения без ошибок связей!
    _context.SaveChanges();

    return Ok(sector);
}
    }
}