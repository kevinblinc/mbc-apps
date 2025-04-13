namespace Shared.Logging;

public interface ILogPublisher
{
    Task LogErrorAsync(string message, Exception? ex = null);
}
