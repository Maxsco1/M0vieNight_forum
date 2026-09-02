using System.Net;

namespace Forum.Domain.Exceptions;

public sealed class EntityAlreadyExistsException(string entityType, string fieldName, object fieldValue) : BaseForumException($"A(n) {entityType} with {fieldName} \"{fieldValue}\" already exists.", HttpStatusCode.Conflict)
{
}