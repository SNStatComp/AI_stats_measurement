using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AI_stats_measurement.Data;
using AI_stats_measurement.Backend.Dto;
using AI_stats_measurement.Backend.Models;

namespace AI_stats_measurement.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PromptsController : ControllerBase
    {
        private readonly AIMeasureDbContext _context;

        public PromptsController(AIMeasureDbContext context)
        {
            _context = context;
        }

        // GET: api/Prompts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Prompt>>> GetPrompts()
        {
            return await _context.Prompts.ToListAsync();
        }

        // GET: api/Prompts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Prompt>> GetPrompt(int id)
        {
            var prompt = await _context.Prompts.FindAsync(id);

            if (prompt == null)
            {
                return NotFound();
            }

            return prompt;
        }

        // PUT: api/Prompts/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPrompt(int id, Prompt prompt)
        {
            if (id != prompt.Id)
            {
                return BadRequest();
            }

            _context.Entry(prompt).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PromptExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Prompts
        [HttpPost]
        public async Task<ActionResult<IEnumerable<int>>> PostPrompts([FromBody] List<PromptDto> dtos)
        {
            if (dtos == null || dtos.Count == 0)
                return BadRequest("No prompts provided.");

            var prompts = new List<Prompt>();

            foreach (var dto in dtos)
            {
                var sourceName = string.IsNullOrWhiteSpace(dto.SourceName) ? null : dto.SourceName.Trim();
                var sourceType = string.IsNullOrWhiteSpace(dto.SourceType) ? null : dto.SourceType.Trim();
                var sourceUrl = string.IsNullOrWhiteSpace(dto.SourceUrl) ? null : dto.SourceUrl.Trim().TrimEnd('/');

                var source = await _context.Sources.FirstOrDefaultAsync(s =>
                    s.Name == sourceName &&
                    s.Url == sourceUrl);

                if (source == null)
                {
                    source = new Source
                    {
                        Name = sourceName,
                        Type = sourceType,
                        Url = sourceUrl
                    };

                    _context.Sources.Add(source);
                }

                var prompt = new Prompt(
                    dto.Provider,
                    dto.Instruction,
                    dto.Theme,
                    dto.Periode,
                    dto.Subject,
                    dto.Question,
                    dto.Answer,
                    source,
                    dto.AnswerLocation
                );

                foreach (var dimension in dto.Dimensions)
                {
                    prompt.AddDimension(dimension.Key, dimension.Value);
                }

                prompts.Add(prompt);
            }

            _context.Prompts.AddRange(prompts);
            await _context.SaveChangesAsync();

            return Ok(prompts.Select(p => p.Id));
        }

        // DELETE: api/Prompts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePrompt(int id)
        {
            var prompt = await _context.Prompts.FindAsync(id);
            if (prompt == null)
            {
                return NotFound();
            }

            _context.Prompts.Remove(prompt);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PromptExists(int id)
        {
            return _context.Prompts.Any(e => e.Id == id);
        }
    }
}
