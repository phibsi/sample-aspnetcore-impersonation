using System;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Sample.AspNetCore.Impersonation.Services
{
    public class ImpersonationService
    {
        private readonly ILogger<ImpersonationService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private HttpContext Context => _httpContextAccessor.HttpContext;

        public ImpersonationService(ILogger<ImpersonationService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public void Impersonate(Action action)
        {
            _logger.LogTrace("BEGIN Impersonate");
            _logger.LogTrace("PARAMETER action: {0}", action);

            _logger.LogInformation("Impersonating user {0}", Context.User.Identity?.Name);

            try
            {
                var user = (WindowsIdentity)Context.User.Identity;

                try
                {
                    WindowsIdentity.RunImpersonated(user.AccessToken, action);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error while impersonating user as WindowsIdentity");
                    throw;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while getting user as WindowsIdentity");
                throw;
            }

            _logger.LogTrace("END Impersonate");
        }

        public T Impersonate<T>(Func<T> action)
        {
            _logger.LogTrace("BEGIN Impersonate");
            _logger.LogTrace("PARAMETER action: {0}", action);

            T actionResult = default;

            try
            {
                var user = (WindowsIdentity)Context.User.Identity;

                try
                {
                    actionResult = WindowsIdentity.RunImpersonated(user.AccessToken, action);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error while impersonating user as WindowsIdentity");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while getting user as WindowsIdentity");
            }

            _logger.LogTrace("END Impersonate");
            _logger.LogTrace("RETURN {0}", actionResult);

            return actionResult;
        }
    }
}
