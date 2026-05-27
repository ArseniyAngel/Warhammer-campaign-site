using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CampaignApp.Data;
using CampaignApp.Models;

namespace CampaignApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsController : ControllerBase
    {
        private readonly CampaignContext _context;

        public NewsController(CampaignContext context)
        {
            _context = context;
        }

        // Получить все новости (Доступно всем, даже Гостям)
        [HttpGet]
        public IActionResult GetNews()
        {
            var news = _context.NewsPosts.OrderByDescending(n => n.CreatedAt).ToList();
            return Ok(news);
        }

        // Добавить новость (Только для Гейм-Мастера)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateNews([FromBody] NewsPost post)
        {
            post.CreatedAt = DateTime.Now;
            _context.NewsPosts.Add(post);
            _context.SaveChanges();
            return Ok(post);
        }
    }
}