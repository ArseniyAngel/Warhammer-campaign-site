using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore; // Нужно для использования асинхронных методов БД (ToListAsync)
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

        // Внедрение зависимости (Dependency Injection) контекста базы данных через конструктор
        public NewsController(CampaignContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Получить все новости.
        /// Доступно абсолютно всем пользователям (включая неавторизованных гостей).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetNews()
        {
            // Извлекаем новости из БД, сортируем по дате (сначала свежие)
            // Используем асинхронный метод ToListAsync(), чтобы не блокировать потоки сервера
            var news = await _context.NewsPosts
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return Ok(news); // Возвращаем статус 200 OK и список новостей в формате JSON
        }

        /// <summary>
        /// Добавить новую новость.
        /// Доступно строго только Гейм-Мастеру (пользователю с ролью Admin).
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")] // Встроенная проверка роли. Если токен не "Admin", вернет 403 Forbidden
        public async Task<IActionResult> CreateNews([FromBody] NewsPost post)
        {
            // Академический стандарт: Валидация входных данных на стороне сервера (Защита от пустых постов)
            if (post == null)
            {
                return BadRequest("Данные новости не были переданы.");
            }

            if (string.IsNullOrWhiteSpace(post.Title) || string.IsNullOrWhiteSpace(post.Content))
            {
                return BadRequest("Заголовок и содержание новости не могут быть пустыми.");
            }

            // Автоматически выставляем серверное время создания новости (чтобы админ не мог подделать дату)
            post.CreatedAt = DateTime.UtcNow;

            // Добавляем объект в контекст Entity Framework
            await _context.NewsPosts.AddAsync(post);
            
            // Сохраняем изменения в PostgreSQL асинхронно
            await _context.SaveChangesAsync();

            // Возвращаем статус 200 OK и созданную новость с присвоенным базой данных Id
            return Ok(post);
        }
    }
}