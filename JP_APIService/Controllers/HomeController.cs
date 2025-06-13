using Microsoft.AspNetCore.Mvc;

namespace JP_APIService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : Controller
    {
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return Ok();
        }
    }
}
