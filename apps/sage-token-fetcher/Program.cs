using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SageTokenManager;

class Program
{
    static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddUserSecrets<Program>()
            .Build();

        string? clientId = config["Sage:ClientId"];
        string? clientSecret = config["Sage:ClientSecret"];
        string? redirectUri = config["Sage:RedirectUri"];

        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret) || string.IsNullOrWhiteSpace(redirectUri))
        {
            Console.WriteLine("Missing Sage OAuth credentials. Please configure user secrets.");
            return;
        }

        Console.WriteLine("Go to this URL and login:");
        var authUrl = $"https://www.sageone.com/oauth2/auth/central" +
                      $"?response_type=code" +
                      $"&client_id={clientId}" +
                      $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                      $"&scope=full_access" +
                      $"&filter=apiv3.1";

        Console.WriteLine(authUrl);

        Console.Write("Paste the code from the URL redirect here: ");
        string? code = Uri.UnescapeDataString(Console.ReadLine() ?? string.Empty);


        if (string.IsNullOrWhiteSpace(code))
        {
            Console.WriteLine("No code entered. Exiting.");
            return;
        }

        using var http = new HttpClient();

        var tokenRequest = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "code", code },
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "redirect_uri", redirectUri }
        };

        var response = await http.PostAsync("https://oauth.accounting.sage.com/token", new FormUrlEncodedContent(tokenRequest));
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var token = new TokenState
        {
            AccessToken = root.GetProperty("access_token").GetString() ?? string.Empty,
            RefreshToken = root.GetProperty("refresh_token").GetString() ?? string.Empty,
            ExpiresAt = DateTime.UtcNow.AddSeconds(root.GetProperty("expires_in").GetInt32())
        };

        var store = new FileTokenStore();
        await store.SaveAsync(token);

        Console.WriteLine("✅ Token received and saved successfully.");
    }
}
