using Microsoft.AspNetCore.Mvc;

namespace perenne.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApiTestController : ControllerBase
    {
        // [host]/api/apitest/ping
        [HttpGet("ping")]
        public ActionResult<string> CheckConnection() => "pong";
    }
}