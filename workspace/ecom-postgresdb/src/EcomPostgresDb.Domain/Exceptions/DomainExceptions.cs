namespace EcomPostgresDb.Domain.Exceptions;

public sealed class DomainException(string message) : Exception(message);

public sealed class NotFoundException(string entity, object key)
    : Exception($"{entity} with key '{key}' was not found.");

public sealed class ConflictException(string message) : Exception(message);

public sealed class BusinessRuleViolationException(string rule) : Exception(rule);
