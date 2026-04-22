using AI_stats_measurement.Backend.Dto;
using AI_stats_measurement.Backend.Models;
using AI_stats_measurement.Data;
using AI_stats_measurement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AI_stats_measurement.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataTransferController : ControllerBase
    {
        private readonly AIMeasureDbContext _context;

        public DataTransferController(AIMeasureDbContext context)
        {
            _context = context;
        }

        [HttpGet("export")]
        public async Task<ActionResult<DataExportBundleDto>> Export()
        {
            var sources = await _context.Sources
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .Select(x => new SourceTransferDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Url = x.Url,
                    Type = x.Type
                })
                .ToListAsync();

            var promptEntities = await _context.Prompts
                .AsNoTracking()
                .Include(x => x.Source)
                .Include(x => x.Dimensions)
                .OrderBy(x => x.Id)
                .ToListAsync();

            var prompts = promptEntities
                .Select(x => new PromptTransferDto
                {
                    Id = x.Id,
                    Provider = x.Provider,
                    Instruction = x.Instruction,
                    Theme = x.Theme,
                    Periode = x.Periode,
                    Subject = x.Subject,
                    Question = x.Question,
                    Answer = x.Answer,
                    SourceId = x.SourceId,
                    SourceName = x.Source?.Name ?? "",
                    SourceType = x.Source?.Type ?? "",
                    SourceUrl = x.Source?.Url ?? "",
                    AnswerLocation = x.AnswerLocation,
                    Dimensions = x.Dimensions.ToDictionary(d => d.Name, d => d.Value),
                    CreatedUtc = x.CreatedUtc
                })
                .ToList();

            var promptDimensions = await _context.Set<PromptDimension>()
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .Select(x => new PromptDimensionTransferDto
                {
                    Id = x.Id,
                    PromptId = x.PromptId,
                    Name = x.Name,
                    Value = x.Value
                })
                .ToListAsync();

            var modelResponses = await _context.Set<ModelResponse>()
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .Select(x => new ModelResponseTransferDto
                {
                    Id = x.Id,
                    PromptId = x.PromptId,
                    Provider = x.Provider,
                    RawText = x.RawText,
                    Exception = x.Exception,
                    CreatedUtc = x.CreatedUtc
                })
                .ToListAsync();

            var bundle = new DataExportBundleDto
            {
                Sources = sources,
                Prompts = prompts,
                PromptDimensions = promptDimensions,
                ModelResponses = modelResponses
            };

            return Ok(bundle);
        }

        [HttpPost("import")]
        public async Task<IActionResult> Import([FromBody] DataExportBundleDto bundle)
        {
            if (bundle == null)
                return BadRequest("Bundle is null.");

            await using var transaction = await _context.Database.BeginTransactionAsync();

            foreach (var dto in bundle.Sources.OrderBy(x => x.Id))
            {
                if (await _context.Sources.AnyAsync(x => x.Id == dto.Id))
                    continue;

                _context.Sources.Add(new Source
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    Url = dto.Url,
                    Type = dto.Type
                });
            }

            await _context.SaveChangesAsync();

            foreach (var dto in bundle.Prompts.OrderBy(x => x.Id))
            {
                if (await _context.Prompts.AnyAsync(x => x.Id == dto.Id))
                    continue;

                var prompt = Prompt.Import(
                    dto.Id,
                    dto.Provider,
                    dto.Instruction,
                    dto.Theme,
                    EnsureUtc(dto.Periode),
                    dto.Subject,
                    dto.Question,
                    dto.Answer,
                    dto.SourceId,
                    dto.AnswerLocation,
                    EnsureUtc(dto.CreatedUtc)
                );

                _context.Prompts.Add(prompt);
            }

            await _context.SaveChangesAsync();

            foreach (var dto in bundle.PromptDimensions.OrderBy(x => x.Id))
            {
                if (await _context.Set<PromptDimension>().AnyAsync(x => x.Id == dto.Id))
                    continue;

                _context.Set<PromptDimension>().Add(
                    PromptDimension.Import(dto.Id, dto.PromptId, dto.Name, dto.Value)
                );
            }

            await _context.SaveChangesAsync();

            foreach (var dto in bundle.ModelResponses.OrderBy(x => x.Id))
            {
                if (await _context.Set<ModelResponse>().AnyAsync(x => x.Id == dto.Id))
                    continue;

                _context.Set<ModelResponse>().Add(
                    ModelResponse.Import(
                        dto.Id,
                        dto.PromptId,
                        dto.Provider,
                        dto.RawText,
                        dto.Exception,
                        EnsureUtc(dto.CreatedUtc)
                    )
                );
            }

            await _context.SaveChangesAsync();

            await ResetSequenceAsync("Sources", "Id");
            await ResetSequenceAsync("Prompts", "Id");
            await ResetSequenceAsync("PromptDimensions", "Id");
            await ResetSequenceAsync("ModelResponses", "Id");

            await transaction.CommitAsync();

            return Ok();
        }

        private static DateTime EnsureUtc(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
            };
        }

        private async Task ResetSequenceAsync(string tableName, string columnName)
        {
            var sql = $@"
                SELECT setval(
                    pg_get_serial_sequence('""{tableName}""', '{columnName}'),
                    COALESCE((SELECT MAX(""{columnName}"") FROM ""{tableName}""), 1),
                    true
                );";

            await _context.Database.ExecuteSqlRawAsync(sql);
        }
    }
}