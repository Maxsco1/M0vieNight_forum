using System.Net;

namespace Forum.Domain.Exceptions;

public abstract class BaseForumException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : Exception(message)
{
    public HttpStatusCode Status { get; set; } = statusCode;
}