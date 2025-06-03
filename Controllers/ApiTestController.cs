using Microsoft.AspNetCore.Mvc;

namespace perenne.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        // [host]/api/apitest/ping
        [HttpGet("ping")]
        public ActionResult<string> CheckConnection() => "pong";
    }
}