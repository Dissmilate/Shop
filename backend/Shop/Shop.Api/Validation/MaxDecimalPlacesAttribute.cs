using System.ComponentModel.DataAnnotations;

namespace Shop.Api.Validation;

public sealed class MaxDecimalPlacesAttribute(int places) : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value is decimal number && decimal.Round(number, places) != number)
            return new ValidationResult($"The field {context.DisplayName} must have at most {places} decimal places.");

        return ValidationResult.Success;
    }
}