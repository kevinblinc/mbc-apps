using System.Text.Json;
using Microsoft.Extensions.Configuration;


namespace SageTokenManager;

public class TokenManager : ISageTokenProvider
{
    private readonly IConfiguration _config;
    private readonly ITokenStore _store;
    private TokenState? _token;
    private static readonly HttpClient Http = new();

    public TokenManager(ITokenStore store)
    {
        _store = store;

        _config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddUserSecrets<TokenManager>()
            .AddEnvironmentVariables()
            .Build();
    }

    public async Task<string> GetAccessTokenAsync()
    {
        _token ??= await _store.LoadAsync();

        if (_token == null || _token.ExpiresAt <= DateTime.UtcNow.AddMinutes(1))
        {
            await RefreshAccessTokenAsync();
        }

        return _token?.AccessToken ?? throw new InvalidOperationException("Access token not available.");

    }

    private async Task RefreshAccessTokenAsync()
    {
        var clientId = _config["Sage:ClientId"];
        var clientSecret = _config["Sage:ClientSecret"];

        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
            throw new InvalidOperationException("ClientId or ClientSecret is missing in config.");

        if (_token == null)
            throw new InvalidOperationException("No existing token found to refresh.");

        var data = new Dictionary<string, string>
        {
            { "grant_type", "refresh_token" },
            { "refresh_token", _token.RefreshToken },
            { "client_id", clientId },
            { "client_secret", clientSecret }
        };

        var response = await Http.PostAsync("https://oauth.accounting.sage.com/token", new FormUrlEncodedContent(data));
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        _token = new TokenState
        {
            AccessToken = root.GetProperty("access_token").GetString() ?? string.Empty,
            RefreshToken = root.GetProperty("refresh_token").GetString() ?? string.Empty,
            ExpiresAt = DateTime.UtcNow.AddSeconds(root.GetProperty("expires_in").GetInt32())
        };

        await _store.SaveAsync(_token);
        Console.WriteLine($"[{DateTime.Now}] Refreshed Sage access token.");
    }
}
