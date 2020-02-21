using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using Sample.AspNetCore.Impersonation.Models;
using Sample.AspNetCore.Impersonation.Services;

namespace Sample.AspNetCore.Impersonation.Controllers
{
    [Route("")]
    public class HomeController : BaseController
    {
        private readonly ImpersonationService _impersonationService;
        private readonly IWebHostEnvironment _environment;

        public HomeController(ImpersonationService impersonationService, IWebHostEnvironment environment)
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

        [HttpGet("userInfoAsync")]
        public async Task<IActionResult> GetUserInfoAsync([FromQuery] string spHostUrl)
        {
            var sb = new StringBuilder();
            try
            {
                sb.AppendLine("## Application pool");
                await GetUserInfoStatusAsync(sb, spHostUrl);

                await _impersonationService.Impersonate(async () =>
                {
                    sb.AppendLine();
                    sb.AppendLine("## Impersonated user");
                    await GetUserInfoStatusAsync(sb, spHostUrl);
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

        private async Task GetUserInfoStatusAsync(StringBuilder sb, string webUrl)
        {
            sb.AppendLine($"User name: {WindowsIdentity.GetCurrent().Name}");
            if (!string.IsNullOrEmpty(webUrl))
            {
                var restResult = await GetSharePointLoginNameAsync(webUrl);
                sb.AppendLine($"SharePoint REST login name: {restResult}");
                var csomResult = await GetSharePointLoginNameCsom(webUrl);
                sb.AppendLine($"SharePoint CSOM login name: {csomResult}");
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

        private async Task<string> GetSharePointLoginNameAsync(string webUrl)
        {
            try
            {
                var url = $"{webUrl.TrimEnd('/')}/_api/Web/CurrentUser";

                var handler = new HttpClientHandler()
                {
                    UseDefaultCredentials = true
                };
                using (var client = new HttpClient(handler))
                {
                    client.DefaultRequestHeaders.Add("Accept", "application/json;odata=verbose");

                    var response = await client.GetAsync(url);
                    var responseJson = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception(responseJson);
                    }

                    var result = JsonConvert.DeserializeObject<CurrentUserResult>(responseJson);

                    return result.D.LoginName;
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        private async Task<string> GetSharePointLoginNameCsom(string webUrl)
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
