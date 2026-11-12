namespace Restaurants.Application.Common.Validation;

public static class PlainTextRules
{
    public static bool IsPlainText(string? value) => value is null || value.All(character =>
        character is not '<' and not '>' &&
        (!char.IsControl(character) || character is '\r' or '\n' or '\t'));
}
