using Erik.Configurations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Erik.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConfigController : ControllerBase
    {
        private readonly ApexConfiguration _apexSettings;

        public ConfigController(IOptions<ApexConfiguration> statusOptions)
        {
            _apexSettings = statusOptions.Value;
        }

        [HttpGet(Name = "apex")]
        public List<string> Get()
        {
            return _apexSettings.Characters;
        }
    }
}