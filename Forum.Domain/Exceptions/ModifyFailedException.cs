using System.Net;

namespace Forum.Domain.Exceptions;

public sealed class ModifyFailedException(string action, string entityType)
: BaseForumException($"Failed to {action} any {entityType}s.", HttpStatusCode.InternalServerError)
{
}