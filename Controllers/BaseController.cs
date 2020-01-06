using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sample.AspNetCore.Impersonation.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public abstract class BaseController : ControllerBase
    {
    }
}
