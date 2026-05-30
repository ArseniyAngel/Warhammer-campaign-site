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

        // 1. МЕТОД ПОЛУЧЕНИЯ КАРТЫ: Берёт абсолютно ВСЕ сектора напрямую из БД
        [HttpGet]
        public ActionResult<IEnumerable<Sector>> GetMap()
        {
            var dbSectors = _context.Sectors.ToList();
            return Ok(dbSectors);
        }

        // Вспомогательный класс (DTO) для безопасного приёма изменений из ГМ-панели
        public class SectorUpdateDto
        {
            // ДОБАВИЛИ: Поле для приёма нового имени от фронтенда
            [JsonPropertyName("name")]
            public string Name { get; set; }

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

            // ОБНОВЛЯЕМ НАЗВАНИЕ СЕКТОРА:
            // Если ГМ прислал не пустое имя, перезаписываем его
            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                sector.Name = dto.Name.Trim();
            }

            // Перевод фракции в null, если выбрана "Нейтральная земля" (0)
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
            // Твой старый комментарий про SQLite меняем мысленно на: Теперь Neon (Postgres) сохранит всё в облако!
            sector.GMMarks = dto.GMMarks;

            _context.SaveChanges();

            return Ok(sector);
        }
    }
}