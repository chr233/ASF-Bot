namespace ASF_Bot.Infrastructure.Exceptions;
public sealed class ConfigNotValidException(string message) : Exception(message)
{
}
