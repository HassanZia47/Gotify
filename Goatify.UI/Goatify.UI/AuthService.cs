using System.Net.Http.Headers;
using System.Net.Http.Json;
using Goatify.Core.Models;
using Goatify.UI.Services;

namespace Goatify.UI;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly TokenStorageService _tokenStorage;
    private readonly JwtAuthenticationStateProvider _authenticationStateProvider;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        HttpClient http,
        TokenStorageService tokenStorage,
        JwtAuthenticationStateProvider authenticationStateProvider,
        ILogger<AuthService> logger)
    {
        _http = http;
        _tokenStorage = tokenStorage;
        _authenticationStateProvider = authenticationStateProvider;
        _logger = logger;
    }

    public bool IsAuthenticated => _authenticationStateProvider.IsAuthenticated;
    public string LastError { get; private set; } = "Invalid username or password.";

    public async Task InitializeAsync()
    {
        var token = await _tokenStorage.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            _authenticationStateProvider.MarkUserAsLoggedOut();
            return;
        }

        SetBearerToken(token);
        _authenticationStateProvider.MarkUserAsAuthenticated(token);
    }

    public async Task<bool> Login(string username, string password)
    {
        try
        {
            LastError = "Invalid username or password.";

            var response = await _http.PostAsJsonAsync("api/auth/login", new LoginDTO
            {
                Username = username,
                Password = password
            });

            if (!response.IsSuccessStatusCode)
            {
                LastError = response.StatusCode == System.Net.HttpStatusCode.Unauthorized
                    ? "Invalid username or password."
                    : await response.Content.ReadAsStringAsync();

                return false;
            }

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

            if (string.IsNullOrWhiteSpace(result?.Token))
            {
                LastError = "The server did not return a valid login token.";
                return false;
            }

            await _tokenStorage.SetTokenAsync(result.Token);
            SetBearerToken(result.Token);
            _authenticationStateProvider.MarkUserAsAuthenticated(result.Token);

            return true;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Login failed because the API is unreachable.");
            LastError = "The API is unreachable. Please make sure the server is running.";
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected login error.");
            LastError = "Login failed because of an unexpected error. Please try again.";
            return false;
        }
    }

    public async Task Logout()
    {
        await _tokenStorage.RemoveTokenAsync();
        _http.DefaultRequestHeaders.Authorization = null;
        _authenticationStateProvider.MarkUserAsLoggedOut();
    }

    private void SetBearerToken(string token)
    {
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    private sealed class LoginResponse
    {
        public string? Token { get; set; }
    }
}
