using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AI_stats_measurement.Backend.Models;
using AI_stats_measurement.Data;

namespace AI_stats_measurement.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LlmJobsController : ControllerBase
    {
        private readonly AIMeasureDbContext _context;

        public LlmJobsController(AIMeasureDbContext context)
        {
            _context = context;
        }

        [HttpGet("jobs/{jobId}")]
        public async Task<IActionResult> GetJob(Guid jobId, CancellationToken ct)
        {
            var job = await _context.LlmJobs.FindAsync([jobId], ct);

            if (job == null)
                return NotFound();

            return Ok(job);
        }

        [HttpGet("jobs/{jobId}/result")]
        public async Task<IActionResult> GetJobResult(Guid jobId, CancellationToken ct)
        {
            var job = await _context.LlmJobs.FindAsync([jobId], ct);

            if (job == null)
                return NotFound();

            if (job.Status != "Completed")
                return BadRequest("Job is not completed yet.");

            var modelResponseIds = await _context.ModelResponses
                .Where(r => r.JobId == jobId)
                .Select(r => r.Id)
                .ToListAsync(ct);

            var rows = await _context.ExportRows
                .Where(r => modelResponseIds.Contains(r.ModelResponseId))
                .OrderBy(r => r.Question)
                .ThenBy(r => r.Provider)
                .ToListAsync(ct);

            return Ok(rows);
        }
    }
}
