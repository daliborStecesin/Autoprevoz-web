using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
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
        tipServera.Equals("PRODUKCIONI", StringComparison.OrdinalIgnoreCase)
            ? _config["ApiKeys:SEF:ProdUrl"] ?? "https://efaktura.mfin.gov.rs/api/publicApi"
            : _config["ApiKeys:SEF:DemoUrl"] ?? "https://demoefaktura.mfin.gov.rs/api/publicApi";

    // Public API v2 — koristi ga SAMO Pojedinačna/Zbirna evidencija PDV (Faza B),
    // odvojen base URL od v1 (ostatak modula e-faktura). Paralelan ResolveBaseUrl-u.
    private string ResolveBaseUrlV2(string tipServera) =>
        tipServera.Equals("PRODUKCIONI", StringComparison.OrdinalIgnoreCase)
            ? _config["ApiKeys:SEF:ProdUrlV2"] ?? "https://efaktura.mfin.gov.rs/api/v2/publicApi"
            : _config["ApiKeys:SEF:DemoUrlV2"] ?? "https://demoefaktura.mfin.gov.rs/api/v2/publicApi";

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
        await EnsureSuccess(response);
        return await response.Content.ReadFromJsonAsync<T>(_jsonOpts);
    }

    public async Task<TResult?> PostAsync<TResult, TBody>(
        string apiKey, string tipServera, string endpoint, TBody body)
    {
        var client   = BuildClient(apiKey);
        var url      = $"{ResolveBaseUrl(tipServera)}/{endpoint}";
        var response = await client.PostAsJsonAsync(url, body);
        await EnsureSuccess(response);
        return await response.Content.ReadFromJsonAsync<TResult>(_jsonOpts);
    }

    /// POST sa praznim telom (isto kao stari PurchaseInvoiceIDs — neki SEF endpoint-i
    /// koji izgledaju kao "GET po opsegu datuma" u query stringu zapravo zahtevaju POST
    /// metod sa praznim/irelevantnim telom; GET na njih vraća 405 MethodNotAllowed).
    public async Task<T?> PostEmptyAsync<T>(string apiKey, string tipServera, string endpoint)
    {
        var client   = BuildClient(apiKey);
        var url      = $"{ResolveBaseUrl(tipServera)}/{endpoint}";
        var content  = new StringContent("", Encoding.UTF8, "application/json");
        var response = await client.PostAsync(url, content);
        await EnsureSuccess(response);
        return await response.Content.ReadFromJsonAsync<T>(_jsonOpts);
    }

    public async Task<byte[]> GetBytesAsync(string apiKey, string tipServera, string endpoint)
    {
        var client = BuildClient(apiKey);

        // Isto kao stari GET_NoticePdf (desktop) — bez ovog Accept header-a SEF
        // odbija zahtev za binarni sadržaj (PDF) sa 400 DownloadRecipientsNoticePdfFileFailed.
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

        var url      = $"{ResolveBaseUrl(tipServera)}/{endpoint}";
        var response = await client.GetAsync(url);
        await EnsureSuccess(response);
        return await response.Content.ReadAsByteArrayAsync();
    }

    /// Binarni PDF odgovor gde SEF, umesto gotovog fajla, ume da vrati JSON poruku
    /// (npr. "PDF se generiše, pokušajte kasnije") — tu situaciju prepoznajemo preko
    /// Content-Type i vraćamo null umesto da tu poruku pokušamo dekodirati kao PDF.
    /// Neuspešan status kod i dalje baca izuzetak (isto kao GetStringAsync/EnsureSuccess).
    public async Task<byte[]?> GetPdfBytesAsync(string apiKey, string tipServera, string endpoint)
    {
        var client = BuildClient(apiKey);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

        var url      = $"{ResolveBaseUrl(tipServera)}/{endpoint}";
        var response = await client.GetAsync(url);
        await EnsureSuccess(response);

        var contentType = response.Content.Headers.ContentType?.MediaType ?? "";
        if (!contentType.Contains("pdf", StringComparison.OrdinalIgnoreCase)) return null;

        return await response.Content.ReadAsByteArrayAsync();
    }

    /// Vraća sirov tekst odgovora (npr. XML endpoint koji nije JSON).
    public async Task<string> GetStringAsync(string apiKey, string tipServera, string endpoint)
    {
        var client   = BuildClient(apiKey);
        var url      = $"{ResolveBaseUrl(tipServera)}/{endpoint}";
        var response = await client.GetAsync(url);
        await EnsureSuccess(response);
        return await response.Content.ReadAsStringAsync();
    }

    /// Isto kao GetStringAsync, samo preko SEF Public API v2 (ResolveBaseUrlV2) —
    /// koriste ga isključivo Pojedinačna/Zbirna evidencija PDV (Faza B).
    public async Task<string> GetStringAsyncV2(string apiKey, string tipServera, string endpoint)
    {
        var client   = BuildClient(apiKey);
        var url      = $"{ResolveBaseUrlV2(tipServera)}/{endpoint}";
        var response = await client.GetAsync(url);
        await EnsureSuccess(response);
        return await response.Content.ReadAsStringAsync();
    }

    /// POST koji vraća sirov string odgovora bez EnsureSuccessStatusCode — uspešan
    /// odgovor kod ovog endpointa nije uvek JSON, a greška JESTE JSON (4xx/5xx), pa
    /// pozivalac (servis) sam parsira telo bez obzira na status kod.
    public async Task<string> PostStringAsync(string apiKey, string tipServera, string endpoint, object body)
    {
        var client   = BuildClient(apiKey);
        var url      = $"{ResolveBaseUrl(tipServera)}/{endpoint}";
        var response = await client.PostAsJsonAsync(url, body);
        return await response.Content.ReadAsStringAsync();
    }

    /// POST sirovog XML tela (UBL faktura) — Content-Type application/xml, ne JSON.
    /// Vraća sirov string odgovora bez EnsureSuccessStatusCode (isti duh kao
    /// PostStringAsync) — uspeh JESTE JSON (MiniInvoiceDto), greška TAKOĐE JSON
    /// (ErrorCode/Message), pa pozivalac parsira oba slučaja iz istog tela.
    public async Task<string> PostXmlAsync(string apiKey, string tipServera, string endpoint, string xml)
    {
        var client   = BuildClient(apiKey);
        var url      = $"{ResolveBaseUrl(tipServera)}/{endpoint}";
        using var content = new StringContent(xml, Encoding.UTF8, "application/xml");
        var response = await client.PostAsync(url, content);
        return await response.Content.ReadAsStringAsync();
    }

    /// DELETE (npr. brisanje draft/new izlazne fakture na SEF-u). Vraća uspeh + sirovo
    /// telo odgovora — pozivalac odlučuje da li dalje parsira grešku iz tela.
    public async Task<(bool uspesno, string telo)> DeleteAsync(string apiKey, string tipServera, string endpoint)
    {
        var client   = BuildClient(apiKey);
        var url      = $"{ResolveBaseUrl(tipServera)}/{endpoint}";
        var response = await client.DeleteAsync(url);
        var telo     = await response.Content.ReadAsStringAsync();
        return (response.IsSuccessStatusCode, telo);
    }

    // EnsureSuccessStatusCode() baca grešku bez tela odgovora — SEF u telu vraća
    // konkretan razlog odbijanja (validacija, pogrešan PIB, itd.), pa ga ovde čitamo
    // i uključujemo u poruku pre nego što se izgubi.
    private static async Task EnsureSuccess(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;

        var telo = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException(
            $"SEF API greška {(int)response.StatusCode} ({response.StatusCode}): {telo}");
    }
}
