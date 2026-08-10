using Microsoft.AspNetCore.Mvc;

namespace CBS.BI.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
    {
          status = "healthy",
    service = "CBS BI API"
});
        }
    }
}
