using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace HubSpot.Client;

public class HubSpotClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public HubSpotClient(HttpClient httpClient, string apiKey)
    {
        _httpClient = httpClient;
        _apiKey = apiKey;
        _httpClient.BaseAddress ??= new Uri("https://api.hubapi.com/");
    }

    private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
    {
        var uri = request.RequestUri!;
        var separator = string.IsNullOrEmpty(uri.Query) ? "?" : "&";
        request.RequestUri = new Uri($"{uri}{separator}hapikey={Uri.EscapeDataString(_apiKey)}");
        return await _httpClient.SendAsync(request);
    }

    public async Task<JsonDocument?> GetContactByEmailAsync(string email)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"crm/v3/objects/contacts/{Uri.EscapeDataString(email)}");
        var response = await SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
        var stream = await response.Content.ReadAsStreamAsync();
        return await JsonDocument.ParseAsync(stream);
    }

    public async Task<bool> CreateContactAsync(object contact)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "crm/v3/objects/contacts");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Content = JsonContent.Create(contact);
        var response = await SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateContactAsync(string contactId, object updates)
    {
        var request = new HttpRequestMessage(HttpMethod.Patch, $"crm/v3/objects/contacts/{contactId}");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Content = JsonContent.Create(updates);
        var response = await SendAsync(request);
        return response.IsSuccessStatusCode;
    }
}
