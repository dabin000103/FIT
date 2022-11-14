using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FIT_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AirController : ControllerBase
    {
        [SwaggerOperation(
        Summary = "웹구역에 사용되는 전체 웹컴포넌트를 조회 합니다",
        Description = "웹구역에 사용되는 전체 웹컴포넌트를 조회 합니다",
        OperationId = "ServiceTest",
        Tags = new[] { "ServiceTest", "WebSite Step 0 - 웹컴포넌트 조회" }
        )]
        [HttpGet("ServiceTest")]
        [Produces("application/json")]
        [SwaggerResponse(200, "웹컴포넌트를 조회 합니다.")]
        [SwaggerResponse(400, "웹컴포넌트 조회 불가능.")]
        [SwaggerResponse(404, "웹컴포넌트 조회 불가능.")]
        public async Task<ActionResult> ServiceTest()
        {
            return Ok("Service Staus Normal");
        }
    }
}
