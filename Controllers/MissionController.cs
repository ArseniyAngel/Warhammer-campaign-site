using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CampaignApp.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CampaignApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MissionsController : ControllerBase
    {
        private readonly CampaignContext _context;
        private readonly Random _random = new Random();

        public MissionsController(CampaignContext context)
        {
            _context = context;
        }

        // POST: api/missions/roll-secondary/1 — Кинуть кубик на вторичную миссию для пользователя
        [HttpPost("roll-secondary/{userId}")]
        public async Task<IActionResult> RollSecondaryMission(int userId)
        {
            // Находим пользователя и подгружаем его фракцию
            var user = await _context.Users
                .Include(u => u.Faction)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound("Пользователь не найден");
            if (user.Faction == null) return BadRequest("Пользователь не привязан ни к одной фракции");

            // Бросаем кубик 1д6
            int diceRoll = _random.Next(1, 7);
            string missionName = "";
            string missionDescription = "";

            // Распределяем миссии по фракциям на основе твоих файлов правил
            switch (user.Faction.Name)
            {
                case "Кровавые Ангелы":
                    var baMissions = new Dictionary<int, (string, string)>
                    {
                        { 1, ("Оберегать Человечество", "Выберите отряд пехоты и положите жетон мирных жителей. Получите 1 ОЧ, если в конце игры жетон на столе.") },
                        { 2, ("Контроль Изъяна", "Каждый раз, убивая врага в ближнем бою, проходите тест на Лидерство. Получите 1 ОЧ, если ни один тест не провален.") },
                        { 3, ("Ангелы Смерти", "Получите 1 ОЧ, если хотя бы один твой отряд уничтожит вражеский отряд в ближнем бою в зоне расстановки противника.") },
                        { 4, ("Важный Узел", "Захватите конкретный маркер точки, выбранный оппонентом.") },
                        { 5, ("Кровью Сангвиния", "Получите ОЧ за уничтожение военачальника противника персонажем.") },
                        { 6, ("Стойкость Баала", "Удерживайте свою зону расстановки до конца матча.") }
                    };
                    (missionName, missionDescription) = baMissions[diceRoll];
                    break;

                case "Аэлдари":
                    var aeldariMissions = new Dictionary<int, (string, string)>
                    {
                        { 1, ("Цель пророчества", "Противник выбирает персонажа. Получите 1 ОС, если на конец игры этот персонаж убит.") },
                        { 2, ("Запутать Пророчество", "Получите 1 ОС, если ни один твой персонаж (Character) не был уничтожен.") },
                        { 3, ("Ускользающая тень", "Выбранный отряд должен совершить Fall Back и в этот же ход занять точку. Награда: 1 ОС.") },
                        { 4, ("Выверенный Путь", "Оппонент выбирает точку в No Man's Land. Удерживайте её в конце игры для получения 1 ОС.") },
                        { 5, ("Исполнить любой ценой", "Выполните секретное тактическое действие в центре стола.") },
                        { 6, ("Живой Щит Игниса", "Используйте отряды союзников для прикрытия ключевых позиций.") }
                    };
                    (missionName, missionDescription) = aeldariMissions[diceRoll];
                    break;

                default:
                    // Заглушка для Астра Милитарум и Некронов (можно расширить аналогично)
                    missionName = $"Фракционная миссия №{diceRoll}";
                    missionDescription = "Специфическое задание для удержания секторов Игниса-IV.";
                    break;
            }

            return Ok(new {
                faction = user.Faction.Name,
                diceRoll = diceRoll,
                mission = missionName,
                rules = missionDescription
            });
        }
    }
}