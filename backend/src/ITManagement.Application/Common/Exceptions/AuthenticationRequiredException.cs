namespace ITManagement.Application.Common.Exceptions;

public sealed class AuthenticationRequiredException(string message) : Exception(message);
