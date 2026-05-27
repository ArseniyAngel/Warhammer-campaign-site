using CampaignApp.Models;
using System.Linq;

namespace CampaignApp.Data
{
    public static class DbInitializer
    {
        public static void Initialize(CampaignContext context)
        {
            // Автоматически создает БД, если её нет
            context.Database.EnsureCreated();

            // 1. Инициализация фракций (если пусто)
            if (!context.Factions.Any())
            {
                var factions = new Faction[]
                {
                    new Faction { Name = "Кровавые Ангелы", ResourceName = "Очки Чести (ОЧ)" },
                    new Faction { Name = "Астра Милитарум", ResourceName = "Очки Снабжения (ОС)" },
                    new Faction { Name = "Некроны", ResourceName = "Очки Пробуждения (ОП)" },
                    new Faction { Name = "Аэлдари", ResourceName = "Очки Судьбы (ОС)" }
                };
                context.Factions.AddRange(factions);
                context.SaveChanges();
            }

            // 2. Инициализация секторов карты (если в таблице Sectors пусто)
            if (!context.Sectors.Any())
            {
                var defaultSectors = new Sector[]
                {
                    new Sector 
                    { 
                        Name = "Пустоши Баала (Сектор Альфа)", 
                        ControllingFactionId = 1, 
                        ResourceType = "Очки Чести (ОЧ)", 
                        Coordinates = "0,0 400,0 400,300 0,300" 
                    },
                    new Sector 
                    { 
                        Name = "Гробница Алтын-Хах (Сектор Бета)", 
                        ControllingFactionId = 2, 
                        ResourceType = "Очки Пробуждения (ОП)", 
                        Coordinates = "400,0 800,0 800,300 400,300" 
                    }
                };

                // Обрати внимание: Обращаемся строго к свойству Sectors (во множественном числе)
                context.Sectors.AddRange(defaultSectors);
                context.SaveChanges();
            }

            // 3. Создаем тестового игрока и отряды (если пользователей еще нет)
            if (!context.Users.Any())
            {
                var bloodAngels = context.Factions.FirstOrDefault(f => f.Name == "Кровавые Ангелы");
                if (bloodAngels != null)
                {
                    var testPlayer = new User
                    {
                        Username = "Sanguinius_77",
                        PasswordHash = "hashed_password_here",
                        Role = "Player",
                        FactionId = bloodAngels.Id,
                        FactionPointsBalance = 5
                    };

                    context.Users.Add(testPlayer);
                    context.SaveChanges();

                    var units = new CustomUnit[]
                    {
                        new CustomUnit 
                        { 
                            Name = "Intercessor Squad Alpha", 
                            Type = "Infantry", 
                            PointsValue = 95, 
                            Status = "Active",
                            UserId = testPlayer.Id 
                        },
                        new CustomUnit 
                        { 
                            Name = "Redemptor Dreadnought", 
                            Type = "Vehicle", 
                            PointsValue = 210, 
                            Status = "Out of Action", 
                            UserId = testPlayer.Id 
                        }
                    };

                    context.CustomUnits.AddRange(units);
                    context.SaveChanges();
                }
            }
            if (!context.CrusadeTraits.Any())
                {
                    context.CrusadeTraits.AddRange(
                        new CrusadeTrait { Name = "Поврежденные сервоприводы", Description = "-1 к Движению (M)", Type = "Scar", UnitTypeRestriction = "All", PtsModifier = 0 },
                        new CrusadeTrait { Name = "Сбой машинного духа", Description = "-1 на попадание при стрельбе", Type = "Scar", UnitTypeRestriction = "Vehicle", PtsModifier = 0 },
                        
                        new CrusadeTrait { Name = "Закаленные ветераны", Description = "+1 к Стойкости (T)", Type = "Upgrade", UnitTypeRestriction = "Infantry", PtsModifier = 15 },
                        new CrusadeTrait { Name = "Освященная броня", Description = "Улучшение Сейва на 1", Type = "Upgrade", UnitTypeRestriction = "All", PtsModifier = 20 },
                        new CrusadeTrait { Name = "Дополнительные турели", Description = "+2 выстрела из тяжелого оружия", Type = "Upgrade", UnitTypeRestriction = "Vehicle", PtsModifier = 30 }
                    );
                    context.SaveChanges();
                }
        }
    }
}