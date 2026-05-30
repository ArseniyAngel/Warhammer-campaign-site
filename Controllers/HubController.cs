using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CampaignApp.Data;
using CampaignApp.Models;
using System.Linq;

namespace CampaignApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HubController : ControllerBase
    {
        private readonly CampaignContext _context;

        public HubController(CampaignContext context) { _context = context; }

        // ================= FAQ =================
        [HttpGet("faq")]
        public IActionResult GetFaqs() => Ok(_context.Faqs.ToList());

        [HttpPost("faq")]
        [Authorize(Roles = "Admin")]
        public IActionResult AddFaq([FromBody] CampaignFaq faq)
        {
            _context.Faqs.Add(faq);
            _context.SaveChanges();
            return Ok(faq);
        }

        [HttpDelete("faq/{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteFaq(int id)
        {
            var faq = _context.Faqs.Find(id);
            if (faq == null) return NotFound();
            _context.Faqs.Remove(faq);
            _context.SaveChanges();
            return Ok();
        }

        // ================= РЕГЛАМЕНТ (ПРАВИЛА + ФАЙЛ) =================
        [HttpGet("regulations")]
        public IActionResult GetRegs()
        {
            var reg = _context.CampaignInfos.FirstOrDefault(x => x.Key == "regulations");
            if (reg == null) return Ok(new CampaignInfo { Key = "regulations", Content = "Регламент пуст", FileUrl = "" });
            return Ok(reg);
        }

        [HttpPut("regulations")]
        [Authorize(Roles = "Admin")]
        public IActionResult UpdateRegs([FromBody] CampaignInfo dto)
        {
            var reg = _context.CampaignInfos.FirstOrDefault(x => x.Key == "regulations");
            if (reg == null)
            {
                reg = new CampaignInfo { Key = "regulations", Content = dto.Content, FileUrl = dto.FileUrl };
                _context.CampaignInfos.Add(reg);
            }
            else
            {
                reg.Content = dto.Content;
                reg.FileUrl = dto.FileUrl;
            }
            _context.SaveChanges();
            return Ok(reg);
        }
    }
}