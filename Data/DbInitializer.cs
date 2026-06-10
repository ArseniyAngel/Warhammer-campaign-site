using CampaignApp.Models;
using System.Linq;

namespace CampaignApp.Data
{
    public static class DbInitializer
    {
        public static void Initialize(CampaignContext context)
        {
            context.Database.EnsureCreated();

            if (!context.Factions.Any())
            {
                var factions = new Faction[]
                {
                    new Faction { Name = "Кровавые Ангелы",  ResourceName = "Очки Чести (ОЧ)"       },
                    new Faction { Name = "Астра Милитарум",  ResourceName = "Очки Снабжения (ОС)"   },
                    new Faction { Name = "Некроны",          ResourceName = "Очки Пробуждения (ОП)" },
                    new Faction { Name = "Аэлдари",          ResourceName = "Очки Судьбы (ОС)"      }
                };
                context.Factions.AddRange(factions);
                context.SaveChanges();
            }

            
            if (!context.Sectors.Any())
            {
                var sectors = new Sector[]
                {
                    new Sector
                    {
                        Name                 = "Пустоши Баала (Сектор Альфа)",
                        ControllingFactionId = 1,
                        ResourceType         = "Очки Чести (ОЧ)",
                        Coordinates          = "0,0 400,0 400,300 0,300",
                        MissionName          = "Удержать позицию",
                        MissionStatus        = "active",
                        IsUnderground        = false
                    },
                    new Sector
                    {
                        Name                 = "Гробница Алтын-Хах (Сектор Бета)",
                        ControllingFactionId = 3,
                        ResourceType         = "Очки Пробуждения (ОП)",
                        Coordinates          = "400,0 800,0 800,300 400,300",
                        MissionName          = "Пробуждение древних",
                        MissionStatus        = "active",
                        IsUnderground        = false
                    }
                };
                context.Sectors.AddRange(sectors);
                context.SaveChanges();
            }

            
            if (!context.Users.Any())
            {
                var admin = new User
                {
                    Username             = "GameMaster",
                    PasswordHash         = "admin",
                    Role                 = "Admin",
                    FactionId            = null,
                    FactionPointsBalance = 0
                };
                context.Users.Add(admin);
                context.SaveChanges();

                var testPlayer = new User
                {
                    Username             = "Sanguinius_77",
                    PasswordHash         = "test",
                    Role                 = "Player",
                    FactionId            = 1,
                    FactionPointsBalance = 5
                };
                context.Users.Add(testPlayer);
                context.SaveChanges();

                var squads = new Squad[]
                {
                    new Squad
                    {
                        CustomName  = "Альфа-отряд",
                        UnitType    = "Intercessors",
                        Type        = "Infantry",
                        PointsCost  = 95,
                        UserId      = testPlayer.Id
                    },
                    new Squad
                    {
                        CustomName  = "Искупитель",
                        UnitType    = "Redemptor Dreadnought",
                        Type        = "Vehicle",
                        PointsCost  = 210,
                        UserId      = testPlayer.Id
                    }
                };
                context.Squads.AddRange(squads);
                context.SaveChanges();
            }

            if (!context.CrusadeTraits.Any())
            {
                context.CrusadeTraits.AddRange(
                    new CrusadeTrait
                    {
                        Name                = "Повреждённые сервоприводы",
                        Description         = "-1 к Движению (M)",
                        Type                = "Scar",
                        UnitTypeRestriction = "All",
                        PtsModifier         = 0,
                        FractionPointsCost  = 0,
                        FactionName         = "All"
                    },
                    new CrusadeTrait
                    {
                        Name                = "Сбой машинного духа",
                        Description         = "-1 на попадание при стрельбе",
                        Type                = "Scar",
                        UnitTypeRestriction = "Vehicle",
                        PtsModifier         = 0,
                        FractionPointsCost  = 0,
                        FactionName         = "All"
                    },

                    
                    new CrusadeTrait
                    {
                        Name                = "Закалённые ветераны",
                        Description         = "+1 к Стойкости (T)",
                        Type                = "Upgrade",
                        UnitTypeRestriction = "Infantry",
                        PtsModifier         = 15,
                        FractionPointsCost  = 3,
                        FactionName         = "All"
                    },
                    new CrusadeTrait
                    {
                        Name                = "Освящённая броня",
                        Description         = "Улучшение Сейва на 1",
                        Type                = "Upgrade",
                        UnitTypeRestriction = "All",
                        PtsModifier         = 20,
                        FractionPointsCost  = 4,
                        FactionName         = "All"
                    },
                    new CrusadeTrait
                    {
                        Name                = "Дополнительные турели",
                        Description         = "+2 выстрела из тяжёлого оружия",
                        Type                = "Upgrade",
                        UnitTypeRestriction = "Vehicle",
                        PtsModifier         = 30,
                        FractionPointsCost  = 5,
                        FactionName         = "All"
                    }
                );
                context.SaveChanges();
            }
        }
    }
}