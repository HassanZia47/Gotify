using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components;

namespace Goatify.UI.Services;

public sealed class AppApiClient
{
    private readonly HttpClient _http;
    private readonly NavigationManager _navigation;
    private readonly ILogger<AppApiClient> _logger;

    public AppApiClient(
        HttpClient http,
        NavigationManager navigation,
        ILogger<AppApiClient> logger)
    {
        _http = http;
        _navigation = navigation;
        _logger = logger;
    }

    public Task<ApiResult<T>> GetAsync<T>(string url, CancellationToken cancellationToken = default)
    {
        return SendAsync<T>(() => _http.GetAsync(url, cancellationToken));
    }

    public Task<ApiResult<T>> PostAsync<T>(string url, object data, CancellationToken cancellationToken = default)
    {
        return SendAsync<T>(() => _http.PostAsJsonAsync(url, data, cancellationToken));
    }

    public Task<ApiResult<T>> PutAsync<T>(string url, object data, CancellationToken cancellationToken = default)
    {
        return SendAsync<T>(() => _http.PutAsJsonAsync(url, data, cancellationToken));
    }

    public Task<ApiResult<bool>> DeleteAsync(string url, CancellationToken cancellationToken = default)
    {
        return SendAsync<bool>(() => _http.DeleteAsync(url, cancellationToken), _ => Task.FromResult(true));
    }

    private async Task<ApiResult<T>> SendAsync<T>(
        Func<Task<HttpResponseMessage>> send,
        Func<HttpContent, Task<T?>>? read = null)
    {
        try
        {
            using var response = await send();

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _navigation.NavigateTo("/login", true);
                return ApiResult<T>.Failure("Your session expired. Please sign in again.", (int)response.StatusCode);
            }

            if (!response.IsSuccessStatusCode)
            {
                return ApiResult<T>.Failure(
                    await ReadErrorAsync(response),
                    (int)response.StatusCode);
            }

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return ApiResult<T>.Success(default, (int)response.StatusCode);
            }

            var value = read == null
                ? await response.Content.ReadFromJsonAsync<T>()
                : await read(response.Content);

            return ApiResult<T>.Success(value, (int)response.StatusCode);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "API request timed out.");
            return ApiResult<T>.Failure("The request took too long. Please try again.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "API request failed.");
            return ApiResult<T>.Failure("The server is unreachable. Please check that the API is running.");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "API returned invalid JSON.");
            return ApiResult<T>.Failure("The server returned data in an unexpected format.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected API client error.");
            return ApiResult<T>.Failure("Something went wrong while contacting the server.");
        }
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        var fallback = response.StatusCode switch
        {
            HttpStatusCode.BadRequest => "Please check the entered information and try again.",
            HttpStatusCode.Forbidden => "You do not have permission to perform this action.",
            HttpStatusCode.NotFound => "The requested record was not found.",
            HttpStatusCode.Conflict => "This record conflicts with existing data.",
            _ => "The server could not complete the request."
        };

        var body = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(body))
            return fallback;

        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;

            if (root.TryGetProperty("detail", out var detail))
                return detail.GetString() ?? fallback;

            if (root.TryGetProperty("title", out var title))
                return title.GetString() ?? fallback;

            if (root.ValueKind == JsonValueKind.Array)
            {
                var messages = root.EnumerateArray()
                    .Select(item => item.TryGetProperty("description", out var description)
                        ? description.GetString()
                        : item.ToString())
                    .Where(message => !string.IsNullOrWhiteSpace(message));

                return string.Join(" ", messages);
            }
        }
        catch (JsonException)
        {
            return body.Trim('"');
        }

        return fallback;
    }
}
