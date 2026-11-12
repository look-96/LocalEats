using System.Text;
using Restaurants.Client.Models.Auth;

namespace Restaurants.Client.Models;

/// <summary>
/// Normalizes user-entered text before the client sends it to the API.
/// DataAnnotations and the API still validate the normalized values.
/// This is not an HTML encoder or a substitute for server-side validation.
/// </summary>
public static class ClientInputSanitizer
{
    public static string NormalizeText(string? value, bool allowLineBreaks = false)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        string unicodeNormalized;
        try
        {
            unicodeNormalized = value.Normalize(NormalizationForm.FormC);
        }
        catch (ArgumentException)
        {
            // Malformed UTF-16 can be supplied by a non-browser client. Drop
            // only unpaired surrogate code units, preserving valid pairs.
            var validUnicode = new StringBuilder(value.Length);
            for (var index = 0; index < value.Length; index++)
            {
                var character = value[index];
                if (char.IsHighSurrogate(character))
                {
                    if (index + 1 < value.Length && char.IsLowSurrogate(value[index + 1]))
                    {
                        validUnicode.Append(character).Append(value[++index]);
                    }
                }
                else if (!char.IsLowSurrogate(character))
                {
                    validUnicode.Append(character);
                }
            }

            unicodeNormalized = validUnicode.ToString().Normalize(NormalizationForm.FormC);
        }

        var normalized = unicodeNormalized
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');
        var result = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (!char.IsControl(character) ||
                (allowLineBreaks && character is '\n' or '\t'))
            {
                result.Append(character);
            }
        }

        return result.ToString().Trim();
    }

    public static string? NormalizeOptionalText(string? value)
    {
        var normalized = NormalizeText(value);
        return normalized.Length == 0 ? null : normalized;
    }

    public static void Normalize(RestaurantCreateRequest request)
    {
        request.Name = NormalizeText(request.Name);
        request.Description = NormalizeText(request.Description, allowLineBreaks: true);
        request.Category = NormalizeText(request.Category);
        request.ContactEmail = NormalizeOptionalText(request.ContactEmail);
        request.ContactNumber = NormalizeOptionalText(request.ContactNumber);
        request.City = NormalizeOptionalText(request.City);
        request.Street = NormalizeOptionalText(request.Street);
        request.PostalCode = NormalizeOptionalText(request.PostalCode);
    }

    public static void Normalize(RestaurantUpdateRequest request)
    {
        request.Name = NormalizeText(request.Name);
        request.Description = NormalizeText(request.Description, allowLineBreaks: true);
    }

    public static void Normalize(DishCreateRequest request)
    {
        request.Name = NormalizeText(request.Name);
        request.Description = NormalizeText(request.Description, allowLineBreaks: true);
    }

    public static void Normalize(LoginRequest request) =>
        request.Email = NormalizeText(request.Email);

    public static void Normalize(RegisterRequest request) =>
        request.Email = NormalizeText(request.Email);
}
