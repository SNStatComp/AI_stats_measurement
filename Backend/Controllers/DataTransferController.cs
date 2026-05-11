using AI_stats_measurement.Backend.Dto;
using AI_stats_measurement.Backend.Models;
using AI_stats_measurement.Backend.Services;
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
        private readonly IDataTransferService _dataTransferService;

        public DataTransferController(
            AIMeasureDbContext context,
            IDataTransferService dataTransferService)
        {
            _context = context;
            _dataTransferService = dataTransferService;
        }

        [HttpGet("export/all")]
        public async Task<ActionResult<DataExportAllBundleDto>> ExportAll()
        {
            var bundle = await _dataTransferService.ExportAllAsync();

            return Ok(bundle);
        }

        [HttpGet("export")]
        public async Task<ActionResult<DataExportBundleDto>> Export()
        {
            var bundle = await _dataTransferService.ExportAsync();

            return Ok(bundle);
        }

        [HttpPost("import")]
        public async Task<IActionResult> Import([FromBody] DataExportBundleDto bundle)
        {
            if (bundle == null)
                return BadRequest("Bundle is null.");

            await _dataTransferService.ImportAsync(bundle);

            return Ok();
        }
    }
}