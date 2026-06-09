using Microsoft.EntityFrameworkCore;
using CampaignApp.Models;

namespace CampaignApp.Data
{
    public class CampaignContext : DbContext
    {
        public CampaignContext(DbContextOptions<CampaignContext> options) : base(options) { }

        public DbSet<Sector> Sectors { get; set; } 
        public DbSet<User> Users { get; set; }
        public DbSet<Faction> Factions { get; set; }
        public DbSet<NewsPost> NewsPosts { get; set; }
        public DbSet<Squad> Squads { get; set; }
        public DbSet<CrusadeTrait> CrusadeTraits { get; set; }
        public DbSet<SquadUpgrade> SquadUpgrades { get; set; }
        public DbSet<CampaignInfo> CampaignInfos { get; set; }
        public DbSet<CampaignMission> Missions { get; set; }
        public DbSet<CampaignFaq> Faqs { get; set; }
        }
}