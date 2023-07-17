using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v1/[controller]")]
    public class BaseApiController : ControllerBase
    {

    }
}