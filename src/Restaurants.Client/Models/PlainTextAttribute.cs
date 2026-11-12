using System.ComponentModel.DataAnnotations;

namespace Restaurants.Client.Models;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class PlainTextAttribute : ValidationAttribute
{
    public PlainTextAttribute() : base("Use plain text without markup characters.") { }

    public override bool IsValid(object? value) => value is null || value is string text &&
        text.All(character => character is not '<' and not '>' &&
                              (!char.IsControl(character) || character is '\r' or '\n' or '\t'));
}
