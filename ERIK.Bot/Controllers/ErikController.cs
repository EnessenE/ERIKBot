using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ERIK.Bot.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ErikController : ControllerBase
    {
        private static readonly string[] Summaries =
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<ErikController> _logger;

        public ErikController(ILogger<ErikController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public string Test()
        {
            return "Test?";
        }
    }
}