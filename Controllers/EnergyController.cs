using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StellarBlueAssignment.Services;
using StellarBlueAssignment.Models;

namespace StellarBlueAssignment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnergyController : ControllerBase{
        private readonly EnergyDataService _service;
        private readonly ILogger<EnergyController> _logger;

        public EnergyController(EnergyDataService service, ILogger<EnergyController> logger){
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves MCP data for the specified date range, calculates daily averages, 
        /// and stores the results in the database (upsert).
        /// Example: POST /api/energy/ImportData/2025-09-01/2025-09-28
        /// </summary>
        [HttpPost("ImportData/{dateFrom}/{dateTo}")]
        public async Task<IActionResult> ImportData([FromRoute] DateTime dateFrom, [FromRoute] DateTime dateTo){
            if (dateFrom  > dateTo) {
                return BadRequest(new { error = "`dateFrom` must be <= `dateTo`" });
            }

            try {
                var saved = await _service.ProcessAndStoreAveragesAsync(dateFrom, dateTo);
                return Ok(new { saved });
            }
            catch (Exception ex){
                _logger.LogError(ex, "Error processing energy data from {From} to {To}", dateFrom, dateTo);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Returns the stored daily average energy prices within the range [from,to].
        /// Example: GET /api/energy/GetData/2025-09-01/2025-09-28
        /// </summary>
        [HttpGet("GetData/{dateFrom}/{dateTo}")]
        public async Task<IActionResult> GetData([FromRoute] DateTime dateFrom, [FromRoute] DateTime dateTo)
        {
            if (dateFrom  > dateTo ){
                return BadRequest(new { error = "`dateFrom` must be <= `dateTo`" });
            }

            try{
                var results = await _service.GetAveragesFromDbAsync(dateFrom, dateTo);
                return Ok(results);
            }
            catch (Exception ex){
                _logger.LogError(ex, "Error fetching averages from {From} to {To}", dateFrom, dateTo);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }
}