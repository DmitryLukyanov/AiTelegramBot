using Microsoft.AspNetCore.Mvc;

namespace TelegramBot.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PingController(ILogger<PingController> logger) : ControllerBase
    {
        [HttpGet(Name = "GetPing")]
        public string Get()
        {
            logger.LogInformation("Ping has been called");
            return Guid.NewGuid().ToString();
        }
    }
}
