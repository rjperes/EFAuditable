using Microsoft.AspNetCore.Http;

namespace EFAuditable
{
    public interface IIdentityProvider
    {
        string GetCurrentUser();
    }

    public sealed class WebIdentityProvider : IIdentityProvider
    {
        private readonly HttpContext _httpContext;

        public WebIdentityProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContext = httpContextAccessor.HttpContext;
        }

        public string GetCurrentUser()
        {
            return _httpContext.User?.Identity?.Name ?? string.Empty;
        }
    }

    public sealed class ConsoleIdentityProvider : IIdentityProvider
    {
        public string GetCurrentUser()
        {
            return Thread.CurrentPrincipal?.Identity?.Name ?? string.Empty;
        }
    }

    public abstract class CustomIdentityProvider : IIdentityProvider
    {
        public abstract string GetCurrentUser();
    }
}
