using System.Text.Json;

namespace SageTokenManager;

public class FileTokenStore : ITokenStore
{
    private readonly string _filePath;

    public FileTokenStore(string filePath = "sage.tokens.json")
    {
        _filePath = filePath;
    }

    public async Task<TokenState?> LoadAsync()
    {
        if (!File.Exists(_filePath))
            return null;

        var json = await File.ReadAllTextAsync(_filePath);
        return JsonSerializer.Deserialize<TokenState>(json);
    }

    public async Task SaveAsync(TokenState token)
    {
        var json = JsonSerializer.Serialize(token, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_filePath, json);
    }
}
