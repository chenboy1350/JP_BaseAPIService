using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JP_APIService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController(Serilog.ILogger logger) : Controller
    {
        private readonly Serilog.ILogger _logger = logger;

        [HttpGet("Index")]
        [Authorize]
        public IActionResult GetIndex()
        {
            _logger.Information("TEST LOG Index");
            var result = new
            {
                statusCode = 200,
                msg = "Success"
            };
            return Ok(result);
        }

        [HttpPost("Index")]
        [Authorize]
        public IActionResult InsertIndex()
        {
            _logger.Information("TEST LOG Index");
            var result = new
            {
                statusCode = 200,
                msg = "Success"
            };
            return Ok(result);
        }

        [HttpPut("Index")]
        [Authorize]
        public IActionResult PutIndex()
        {
            _logger.Information("TEST LOG Index");
            var result = new
            {
                statusCode = 200,
                msg = "Success"
            };
            return Ok(result);
        }

        [HttpPatch("Index")]
        [Authorize]
        public IActionResult PatchIndex()
        {
            _logger.Information("TEST LOG Index");
            var result = new
            {
                statusCode = 200,
                msg = "Success"
            };
            return Ok(result);
        }

        [HttpDelete("Index")]
        [Authorize]
        public IActionResult DeleteIndex()
        {
            _logger.Information("TEST LOG Index");
            var result = new
            {
                statusCode = 200,
                msg = "Success"
            };
            return Ok(result);
        }
    }
}
