namespace Forum.Domain.Exceptions;

public sealed class EntityDoesNotExistException(string entityType, string key, object value)
: BaseForumException($"{entityType} with {key} \"{value}\" not found.", System.Net.HttpStatusCode.NotFound)
{
}