namespace LifeCrm.Application.Common.Exceptions
{
    public class NotFoundException : Exception
    {
        public string EntityName { get; }
        public object Key { get; }
        public NotFoundException(string entityName, object key)
            : base($"{entityName} with ID '{key}' was not found.")
        { EntityName = entityName; Key = key; }
    }

    public record ValidationError(string Field, string Message);

    public class ValidationException : Exception
    {
        public IReadOnlyList<ValidationError> Errors { get; }
        public ValidationException(IEnumerable<ValidationError> errors)
            : base("One or more validation errors occurred.")
        { Errors = errors.ToList().AsReadOnly(); }
        public ValidationException(string field, string message)
            : this(new[] { new ValidationError(field, message) }) { }
    }

    public class ForbiddenException : Exception
    {
        public ForbiddenException() : base("You do not have permission to perform this action.") { }
        public ForbiddenException(string message) : base(message) { }
    }

    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
    }
}
