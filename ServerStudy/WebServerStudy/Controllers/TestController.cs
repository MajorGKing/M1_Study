using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebServerStudy.Controllers
{
    [Route("test")]
    [ApiController]
    public class TestController : ControllerBase
    {
        // POST api/<TestController>
        [HttpPost]
        [Route("hello")]
        public TestPacketRes TestPost([FromBody] TestPacketReq value)
        {
            TestPacketRes result = new TestPacketRes();
            result.success = true;

            return result;
        }
    }
}
