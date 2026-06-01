using System.Security.Claims;
using Goatify.Core.Services;

namespace Goatify.API.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserName
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;

            return user?.Identity?.IsAuthenticated == true
                ? user.FindFirstValue(ClaimTypes.Name)
                    ?? user.FindFirstValue(ClaimTypes.NameIdentifier)
                : null;
        }
    }
}
