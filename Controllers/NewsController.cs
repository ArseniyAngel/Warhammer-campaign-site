using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CampaignApp.Data;
using CampaignApp.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

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

        [HttpGet]
        public async Task<IActionResult> GetNews()
        {
    
            var news = await _context.NewsPosts
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return Ok(news);
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateNews([FromBody] NewsPost post)
        {
            
            if (post == null)
            {
                return BadRequest("Данные новости не были переданы.");
            }

            if (string.IsNullOrWhiteSpace(post.Title) || string.IsNullOrWhiteSpace(post.Content))
            {
                return BadRequest("Заголовок и содержание новости не могут быть пустыми.");
            }

    
            post.CreatedAt = DateTime.UtcNow;

        
            await _context.NewsPosts.AddAsync(post);
            
           
            await _context.SaveChangesAsync();

           
            return Ok(post);
        }
    }
}