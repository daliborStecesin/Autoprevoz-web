using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Transport.Application.Services.Sef;

/// <summary>
/// Low-level HTTP wrapper za SEF (Sistem e-faktura) API.
/// Ne poznaje poslovnu logiku — samo šalje zahteve i deserijalizuje odgovore.
/// Dobija apiKey i tipServera od pozivaoca (SefService čita iz tbl_Podesavanja).
/// </summary>
public class SefApiClient
{
    private readonly IHttpClientFactory _factory;
    private readonly IConfiguration     _config;

    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public SefApiClient(IHttpClientFactory factory, IConfiguration config)
    {
        _factory = factory;
        _config  = config;
    }

    private string ResolveBaseUrl(string tipServera) =>
        tipServera.Equals("PRODUKCIJA", StringComparison.OrdinalIgnoreCase)
            ? _config["ApiKeys:SEF:ProdUrl"] ?? "https://efaktura.mfin.gov.rs/api/publicApi"
            : _config["ApiKeys:SEF:DemoUrl"] ?? "https://demoefaktura.mfin.gov.rs/api/publicApi";

    private HttpClient BuildClient(string apiKey)
    {
        var client = _factory.CreateClient("SEF");
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("ApiKey", apiKey);
        return client;
    }

    public async Task<T?> GetAsync<T>(string apiKey, string tipServera, string endpoint)
    {
        var client   = BuildClient(apiKey);
        var url      = $"{ResolveBaseUrl(tipServera)}/{endpoint}";
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(_jsonOpts);
    }

    public async Task<TResult?> PostAsync<TResult, TBody>(
        string apiKey, string tipServera, string endpoint, TBody body)
    {
        var client   = BuildClient(apiKey);
        var url      = $"{ResolveBaseUrl(tipServera)}/{endpoint}";
        var response = await client.PostAsJsonAsync(url, body);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResult>(_jsonOpts);
    }
}
