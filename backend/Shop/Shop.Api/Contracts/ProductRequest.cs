using System.ComponentModel.DataAnnotations;
using Shop.Api.Validation;

namespace Shop.Api.Contracts;

public record ProductRequest(
    [Required, StringLength(200)] string Name,
    [Range(typeof(decimal), "0.01", "9999999999999999.99",
        ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)]
    [MaxDecimalPlaces(2)]
    decimal Price);