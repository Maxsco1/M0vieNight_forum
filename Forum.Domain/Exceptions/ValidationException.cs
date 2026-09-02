using System.Net;

namespace Forum.Domain.Exceptions;

public sealed class ValidationException(IDictionary<string, string[]> errors)
: BaseForumException("One or more validation errors occurred.", HttpStatusCode.BadRequest)
{
    public IDictionary<string, string[]> Errors { get; } = errors;
}