using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace Goatify.UI.Services;

public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private static readonly ClaimsPrincipal AnonymousUser = new(new ClaimsIdentity());
    private ClaimsPrincipal _currentUser = AnonymousUser;

    public bool IsAuthenticated => _currentUser.Identity?.IsAuthenticated == true;

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(new AuthenticationState(_currentUser));
    }

    public void MarkUserAsAuthenticated(string token)
    {
        _currentUser = new ClaimsPrincipal(new ClaimsIdentity(ParseClaims(token), "jwt"));
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void MarkUserAsLoggedOut()
    {
        _currentUser = AnonymousUser;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private static IEnumerable<Claim> ParseClaims(string token)
    {
        var payload = token.Split('.')[1];
        var jsonBytes = Convert.FromBase64String(PadBase64(payload));
        var claims = new List<Claim>();

        using var document = JsonDocument.Parse(jsonBytes);

        foreach (var property in document.RootElement.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.Array)
            {
                claims.AddRange(property.Value.EnumerateArray()
                    .Select(value => CreateClaim(property.Name, value.ToString())));
                continue;
            }

            claims.Add(CreateClaim(property.Name, property.Value.ToString()));
        }

        return claims;
    }

    private static Claim CreateClaim(string type, string value)
    {
        return type switch
        {
            "name" => new Claim(ClaimTypes.Name, value),
            "unique_name" => new Claim(ClaimTypes.Name, value),
            "nameid" => new Claim(ClaimTypes.NameIdentifier, value),
            "role" => new Claim(ClaimTypes.Role, value),
            _ => new Claim(type, value)
        };
    }

    private static string PadBase64(string base64)
    {
        base64 = base64.Replace('-', '+').Replace('_', '/');

        return base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
    }
}
