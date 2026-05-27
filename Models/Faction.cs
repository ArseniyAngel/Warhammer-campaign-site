namespace CampaignApp.Models
{
    public class Faction
    {
        // Первичный ключ для базы данных
        public int Id { get; set; }
        
        // Название фракции (Кровавые Ангелы, Некроны и т.д.)
        public string Name { get; set; } = string.Empty;
        
        // Название уникального ресурса (Очки Чести, Очки Пробуждения)
        public string ResourceName { get; set; } = string.Empty;
    }
}