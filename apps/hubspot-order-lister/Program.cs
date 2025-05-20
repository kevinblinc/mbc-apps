using System.Net.Http.Headers;
using System.Text.Json;

class Program
{
    static async Task Main(string[] args)
    {
        var token = Environment.GetEnvironmentVariable("HUBSPOT_ACCESS_TOKEN");
        if (string.IsNullOrWhiteSpace(token))
        {
            Console.WriteLine("HUBSPOT_ACCESS_TOKEN environment variable not set.");
            return;
        }

        using var http = new HttpClient();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string? after = null;
        do
        {
            var url = "https://api.hubapi.com/crm/v3/objects/sales_orders?limit=100";
            if (!string.IsNullOrEmpty(after))
                url += "&after=" + after;

            var resp = await http.GetAsync(url);
            resp.EnsureSuccessStatusCode();
            var json = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            foreach (var order in root.GetProperty("results").EnumerateArray())
            {
                var orderId = order.GetProperty("id").GetString();
                if (string.IsNullOrEmpty(orderId))
                    continue;

                // Get associated companies
                var assocResp = await http.GetAsync($"https://api.hubapi.com/crm/v4/objects/sales_orders/{orderId}/associations/companies");
                assocResp.EnsureSuccessStatusCode();
                var assocJson = await assocResp.Content.ReadAsStringAsync();
                using var assocDoc = JsonDocument.Parse(assocJson);
                var companies = assocDoc.RootElement.GetProperty("results");

                foreach (var companyAssoc in companies.EnumerateArray())
                {
                    var companyId = companyAssoc.GetProperty("id").GetString();
                    if (string.IsNullOrEmpty(companyId))
                        continue;
                    var companyResp = await http.GetAsync($"https://api.hubapi.com/crm/v3/objects/companies/{companyId}?properties=name");
                    companyResp.EnsureSuccessStatusCode();
                    var companyJson = await companyResp.Content.ReadAsStringAsync();
                    using var compDoc = JsonDocument.Parse(companyJson);
                    var name = compDoc.RootElement.GetProperty("properties").GetProperty("name").GetString();
                    Console.WriteLine(name);
                }
            }

            after = root.TryGetProperty("paging", out var paging) &&
                    paging.TryGetProperty("next", out var next) &&
                    next.TryGetProperty("after", out var afterProp)
                ? afterProp.GetString()
                : null;
        } while (!string.IsNullOrEmpty(after));
    }
}
