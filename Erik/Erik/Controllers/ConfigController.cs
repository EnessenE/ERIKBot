using Erik.Configurations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Erik.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConfigController : ControllerBase
    {
        private readonly StatusConfiguration _statusSettings;

        public ConfigController(IOptions<StatusConfiguration> statusOptions)
        {
            _statusSettings = statusOptions.Value;
        }

        [HttpGet(Name = "apex")]
        public List<string> Get()
        {
            return _statusSettings.Statuses;
        }
    }
}