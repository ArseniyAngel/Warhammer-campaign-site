using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CampaignApp.Data;
using CampaignApp.Models;

namespace CampaignApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UnitsController : ControllerBase
    {
        private readonly CampaignContext _context;
        // Генератор случайных чисел для имитации бросков кубика д6
        private readonly Random _random = new Random();

        public UnitsController(CampaignContext context)
        {
            _context = context;
        }

        // 1. GET: api/units/user/5 — Получить все отряды конкретного игрока
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<CustomUnit>>> GetUserUnits(int userId)
        {
            var units = await _context.CustomUnits
                .Where(u => u.UserId == userId)
                .ToListAsync();
            return Ok(units);
        }

        // 2. POST: api/units/roll-survival/5 — Бросок на выживание (Шаг 1 правил «Шрамы войны»)
        [HttpPost("roll-survival/{unitId}")]
public async Task<IActionResult> RollSurvival(int unitId)
{
    var unit = await _context.CustomUnits.FindAsync(unitId);
    if (unit == null) return NotFound("Юнит не найден");

    // 1. Бросаем первый кубик 1д6
    int firstRoll = _random.Next(1, 7);
    string resultMessage;

    if (firstRoll <= 2) // 1-2: Критическое состояние
    {
        unit.Status = "Out of Action"; // Выведен из строя
        resultMessage = $"Выпало {firstRoll}: Критическое состояние! Отряд выведен из строя на следующий матч.";
    }
    else if (firstRoll <= 4) // 3-4: Шрамы войны (Имитируем бросок д66!)
    {
        // Бросаем два кубика для таблицы д66
        int dice1 = _random.Next(1, 7); // Десятки
        int dice2 = _random.Next(1, 7); // Единицы
        int d66Result = (dice1 * 10) + dice2; // Получаем число (например, 11, 34, 66)

        // Словарь шрамов по твоей таблице правил
        var scarsTable = new Dictionary<int, string>
        {
            { 11, "Контузия (-1 к Лидерству)" },
            { 12, "Повреждение оптики (-1 к броскам на попадание в дальнем бою)" },
            { 13, "Повреждение приводов (-2\" к Муву)" },
            { 14, "Паника перед рукопашной (-1 к атакам в ближнем бою)" },
            { 15, "Разрушение брони (Ухудшение сейва на 1)" },
            { 16, "ПТСР (При провале теста на Боевой Шок отряд теряет 1 модель)" },
            // Для примера заполним другие десятки, ты можешь вписать сюда свои точные тексты:
            { 21, "Поврежденный генератор (Стратагемы на отряд стоят на 1 КП дороже)" },
            { 33, "Осколочные ранения (-1 к Тафне)" },
            { 45, "Хромота (Нельзя чарджить после адванса)" },
            { 52, "Потеря сержанта (-1 к лидерству и контролю точек)" },
            // Самый крутой эффект из твоих правил:
            { 66, "Мгновенное излечение от всех предыдущих шрамов!" }
        };

        // Ищем выпавший шрам. Если в словаре нет такого индекса, ставим стандартный текст
        string scarEffect = scarsTable.ContainsKey(d66Result) 
            ? scarsTable[d66Result] 
            : $"Шрам войны №{d66Result} (Эффект требует уточнения у ГМ)";

        if (d66Result == 66)
        {
            // Очищаем шрамы, если выбросили 66
            unit.BattleScarsAndUpgrades = "[Мгновенно излечен]";
            resultMessage = $"Выпало {firstRoll} (Шрамы войны). Бросок д66 выдал: {d66Result}! {scarEffect}";
        }
        else
        {
            // Добавляем новый шрам к уже существующим
            unit.BattleScarsAndUpgrades += $"[{scarEffect}] ";
            resultMessage = $"Выпало {firstRoll} (Шрамы войны). Бросок д66 выдал: {d66Result}! Отряд получает: {scarEffect}";
        }
    }
    else // 5-6: Легкие ранения
    {
        resultMessage = $"Выпало {firstRoll}: Легкие ранения. Психика стабильна, дебаффов нет!";
    }

    // Сохраняем сгенерированный шрам в базу данных SQLite
    await _context.SaveChangesAsync();

    return Ok(new { 
        firstDiceRoll = firstRoll,
        message = resultMessage, 
        currentStatus = unit.Status, 
        allScars = unit.BattleScarsAndUpgrades 
    });
}

        // 3. POST: api/units/reinforce/5 — Запрос подкреплений (Шаг 2 правил «Шрамы войны»)
        [HttpPost("reinforce/{unitId}")]
        public async Task<IActionResult> RequestReinforcements(int unitId)
        {
            // Подгружаем юнита вместе с его владельцем (User), чтобы проверить баланс ОФ
            var unit = await _context.CustomUnits
                .Include(u => u.User)
                .FirstOrDefaultAsync(u => u.Id == unitId);

            if (unit == null) return NotFound("Юнит не найден");
            if (unit.User == null) return BadRequest("У юнита нет владельца");

            // Вычисляем стоимость возвращения в ОФ по твоей таблице из правил:
            int pointsPrice = unit.PointsValue;
            int factionPointsCost = 0;

            if (pointsPrice <= 100) factionPointsCost = 1;
            else if (pointsPrice <= 250) factionPointsCost = 2;
            else if (pointsPrice <= 400) factionPointsCost = 3;
            else factionPointsCost = 4;

            // Проверяем, хватает ли у игрока ОФ (Очков Фракции)
            if (unit.User.FactionPointsBalance < factionPointsCost)
            {
                return BadRequest($"Недостаточно Очков Фракции. Нужно: {factionPointsCost}, у вас: {unit.User.FactionPointsBalance}");
            }

            // Списываем очки
            unit.User.FactionPointsBalance -= factionPointsCost;
            
            // Логика из правил: "Возвращается базовая версия, снимаются все прокачки и шрамы"
            unit.Status = "Active"; 
            unit.BattleScarsAndUpgrades = string.Empty; 

            await _context.SaveChangesAsync();

            return Ok(new { 
                message = $"Отряд успешно восстановлен! Потрачено {factionPointsCost} ОФ.", 
                remainingFactionPoints = unit.User.FactionPointsBalance,
                unitStatus = unit.Status
            });
        }
    }
}