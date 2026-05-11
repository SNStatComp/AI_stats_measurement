using AI_stats_measurement.Backend.Models;
using AI_stats_measurement.Data;
using AI_stats_measurement.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AI_stats_measurement.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExportRowsController : ControllerBase
    {
        private readonly AIMeasureDbContext _context;

        public ExportRowsController(AIMeasureDbContext context)
        {
            _context = context;
        }

        // GET: api/ExportRows
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExportRow>>> GetExportRows()
        {
            return await _context.ExportRows.ToListAsync();
        }

        // GET: api/ExportRows/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ExportRow>> GetExportRow(int id)
        {
            var exportRow = await _context.ExportRows.FindAsync(id);

            if (exportRow == null)
            {
                return NotFound();
            }

            return exportRow;
        }

        // GET: api/ExportRows/byPrompt/5
        [HttpGet("byPrompt/{promptId}")]
        public async Task<ActionResult<IEnumerable<ExportRow>>> GetExportRowsByPrompt(int promptId)
        {
            var rows = await _context.FactCheckResults
                .Include(f => f.ParsedModelResponse)
                    .ThenInclude(pmr => pmr.ParsedModelResponseSources)
                        .ThenInclude(pmrs => pmrs.Source)
                .Include(f => f.ParsedModelResponse)
                    .ThenInclude(pmr => pmr.ModelResponse)
                        .ThenInclude(mr => mr.Prompt)
                            .ThenInclude(p => p.Source)
                .Where(f => f.ParsedModelResponse.ModelResponse.Prompt.Id == promptId)
                .Select(f => new ExportRow(
                    f.ParsedModelResponse.ModelResponse.Id,
                    f.ParsedModelResponse.ModelResponse.Prompt.Theme,
                    f.ParsedModelResponse.ModelResponse.Prompt.Question,
                    f.ParsedModelResponse.ModelResponse.Prompt.Answer,
                    f.ParsedModelResponse.ModelResponse.Prompt.Source.Url,
                    f.ParsedModelResponse.Answer,
                    f.ParsedModelResponse.ParsedModelResponseSources
                        .Select(s => s.Source.Id)
                        .ToList(),
                    f.ParsedModelResponse.ModelResponse.Provider,
                    f.ParsedModelResponse.ModelResponse.RawText,
                    f.ParsedModelResponse.ModelResponse.Exception,
                    0,
                    f.RelativeError,
                    f.AnswerIsCorrect,
                    f.SourceIsCorrect,
                    f.ParsedModelResponse.ModelResponse.CreatedUtc
                ))
                .ToListAsync();

            return Ok(rows);
        }

        // PUT: api/ExportRows/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutExportRow(int id, ExportRow exportRow)
        {
            if (id != exportRow.Id)
            {
                return BadRequest();
            }

            _context.Entry(exportRow).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExportRowExists(id))
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

        // POST: api/ExportRows
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ExportRow>> PostExportRow(ExportRow exportRow)
        {
            _context.ExportRows.Add(exportRow);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetExportRow", new { id = exportRow.Id }, exportRow);
        }

        // DELETE: api/ExportRows/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExportRow(int id)
        {
            var exportRow = await _context.ExportRows.FindAsync(id);
            if (exportRow == null)
            {
                return NotFound();
            }

            _context.ExportRows.Remove(exportRow);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ExportRowExists(int id)
        {
            return _context.ExportRows.Any(e => e.Id == id);
        }
    }
}
