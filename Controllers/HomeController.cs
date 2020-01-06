using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.SharePoint.Client;
using Sample.AspNetCore.Impersonation.Services;

namespace Sample.AspNetCore.Impersonation.Controllers
{
    [Route("")]
    public class HomeController : BaseController
    {
        private readonly ImpersonationService _impersonationService;
        private readonly IHostingEnvironment _environment;

        public HomeController(ImpersonationService impersonationService, IHostingEnvironment environment)
        {
            _impersonationService = impersonationService;
            _environment = environment;
        }

        [HttpGet]
        public IActionResult GetIndex()
        {
            return Content(typeof(HomeController).Assembly.FullName);
        }

        [HttpGet("userInfo")]
        public async Task<IActionResult> GetUserInfo([FromQuery] string spHostUrl)
        {
            var sb = new StringBuilder();
            try
            {
                sb.AppendLine("## Application pool");
                await GetUserInfoStatus(sb, spHostUrl);

                await _impersonationService.Impersonate(async () =>
                {
                    sb.AppendLine();
                    sb.AppendLine("## Impersonated user");
                    await GetUserInfoStatus(sb, spHostUrl);
                });
            }
            catch (Exception ex) {
                sb.AppendLine("Something went wrong :(");
                sb.AppendLine($"{ex.GetType().FullName}: {ex.Message}\n{ex.StackTrace}");
            }

            // Mask App Pool Identity
            if (_environment.IsProduction())
            {
                var appPool = WindowsIdentity.GetCurrent().Name;
                var appPoolParts = appPool.Split('\\', StringSplitOptions.RemoveEmptyEntries);

                string maskedAppPool = string.Empty;
                string userName;
                if (appPoolParts.Length == 2)
                {
                    maskedAppPool = appPoolParts[0] + "\\";
                    userName = appPoolParts[1];
                }
                else
                {
                    userName = appPool;
                }
                maskedAppPool += userName.Substring(0, 2) + "***";

                sb.Replace(appPool, maskedAppPool);
            }

            return Content(sb.ToString());
        }

        private async Task GetUserInfoStatus(StringBuilder sb, string webUrl)
        {
            sb.AppendLine($"User name: {WindowsIdentity.GetCurrent().Name}");
            if (!string.IsNullOrEmpty(webUrl))
            {
                var result = await GetSharePointLoginName(webUrl);
                sb.AppendLine($"SharePoint login name: {result}");
            }
        }

        private async Task<string> GetSharePointLoginName(string webUrl)
        {
            try
            {
                using (var clientContext = new ClientContext(webUrl))
                {
                    var web = clientContext.Web;
                    var user = web.CurrentUser;
                    clientContext.Load(user, u => u.LoginName);
                    await clientContext.ExecuteQueryAsync();
                    return user.LoginName;
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
    }
}
