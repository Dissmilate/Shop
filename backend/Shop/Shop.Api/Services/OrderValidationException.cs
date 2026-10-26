namespace Shop.Api.Services;

public class OrderValidationException(string message) : Exception(message);