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
        public IActionResult Index()
        {
            _logger.Information("TEST LOG Index");
            return Ok();
        }
    }
}
