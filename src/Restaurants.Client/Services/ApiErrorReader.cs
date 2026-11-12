using System.Text.Json;

namespace Restaurants.Client.Services;

public static class ApiErrorReader
{
    public static async Task<string> ReadAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(body))
        {
            return response.StatusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized => "Please sign in and try again.",
                System.Net.HttpStatusCode.Forbidden => "Your account does not have permission to do that.",
                _ => $"The request failed ({(int)response.StatusCode})."
            };
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;
            if (root.TryGetProperty("detail", out var detail) && detail.ValueKind == JsonValueKind.String)
                return detail.GetString()!;
            if (root.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String)
                return title.GetString()!;
            if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
                return string.Join(" ", errors.EnumerateObject().SelectMany(item => item.Value.EnumerateArray()).Select(item => item.GetString()).Where(item => !string.IsNullOrWhiteSpace(item)));
        }
        catch (JsonException)
        {
            // The API may return a plain text error body.
        }

        return body;
    }
}
