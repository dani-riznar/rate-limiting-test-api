using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RateLimiterTest.Clients;

namespace RateLimiterTest.Controllers
{
    [EnableRateLimiting("fixed")]
    [Route("api/[controller]")]
    [ApiController]
    public class RateLimitController : ControllerBase
    {
        private readonly IRateLimitHttpClient _rateLimitHttpClient;

        public RateLimitController(IRateLimitHttpClient rateLimitHttpClient)
        {
            _rateLimitHttpClient=rateLimitHttpClient;
        }

        [DisableRateLimiting]
        [HttpGet("httpclient")]
        public async Task<IActionResult> GetCallHttpClientLimiting()
        {
            var result= await _rateLimitHttpClient.GetAsync("https://example.com", default(CancellationToken));

            if (result == 200)
            {
                return StatusCode(200);
            }
            else if (result == 429)
            {
                return StatusCode(429);
            }
            else
            {
                return StatusCode(500);
            }
        }

        [EnableRateLimiting("fixed")]
        [HttpGet("middleware")]
        public async Task<IActionResult> GetCallMiddlewareLimiting()
        {
            return StatusCode(200);
        }
    }


}
