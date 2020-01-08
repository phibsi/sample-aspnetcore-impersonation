using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Net;
using System.Security.Principal;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using Sample.AspNetCore.Impersonation.Models;
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
        public IActionResult GetUserInfo([FromQuery] string spHostUrl)
        {
            var sb = new StringBuilder();
            try
            {
                sb.AppendLine("## Application pool");
                GetUserInfoStatus(sb, spHostUrl);

                _impersonationService.Impersonate(() =>
                {
                    sb.AppendLine();
                    sb.AppendLine("## Impersonated user");
                    GetUserInfoStatus(sb, spHostUrl);
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

        private void GetUserInfoStatus(StringBuilder sb, string webUrl)
        {
            sb.AppendLine($"User name: {WindowsIdentity.GetCurrent().Name}");
            if (!string.IsNullOrEmpty(webUrl))
            {
                var result = GetSharePointLoginName(webUrl);
                sb.AppendLine($"SharePoint login name: {result}");
            }
        }

        private string GetSharePointLoginName(string webUrl)
        {
            try
            {
                var url = $"{webUrl.TrimEnd('/')}/_api/Web/CurrentUser";
                
                var request = WebRequest.CreateHttp(url);
                request.UseDefaultCredentials = true;
                request.Method = "GET";
                request.Headers.Add(HttpRequestHeader.Accept, "application/json;odata=verbose");

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (var responseStream = response.GetResponseStream())
                    {
                        if (responseStream == null)
                        {
                            throw new InvalidOperationException();
                        }

                        using (var myStreamReader = new StreamReader(responseStream, Encoding.UTF8))
                        {
                            var responseJson = myStreamReader.ReadToEnd();
                            var result = JsonConvert.DeserializeObject<CurrentUserResult>(responseJson);

                            return result.D.LoginName;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
    }
}
