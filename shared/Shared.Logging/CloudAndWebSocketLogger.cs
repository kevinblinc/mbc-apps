using System.Text.Json;

namespace Shared.Logging;

public class CloudAndWebSocketLogger : ILogPublisher
{
    public async Task LogErrorAsync(string message, Exception? ex = null)
    {
        var log = new
        {
            level = "error",
            message,
            exception = ex?.ToString(),
            timestamp = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(log);
        Console.WriteLine(json);

        // TODO: Add WebSocket forwarding if needed
        await Task.CompletedTask;
    }
}
