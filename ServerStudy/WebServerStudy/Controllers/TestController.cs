using Microsoft.AspNetCore.Mvc;
using WebServerStudy.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebServerStudy.Controllers
{
    [Route("test")]
    [ApiController]
    public class TestController : ControllerBase
    {
        AccountService _service;

        public TestController(AccountService service)
        {
            _service = service;
        }

        // ASP.NET CORE < WEB
        // ENTITY FRAMEWORK CORE < DB(ORM)

        // ip:port/test/hello
        // POST api/<TestController>
        [HttpPost]
        [Route("hello")]
        public TestPacketRes TestPost([FromBody] TestPacketReq value)
        {
            TestPacketRes result = new TestPacketRes();
            result.success = true;

            int id = _service.GenerateAccountId();

            return result;
        }
    }
}
