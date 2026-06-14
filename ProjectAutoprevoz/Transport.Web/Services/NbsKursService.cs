using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Caching.Memory;
using Transport.Application.DTOs;
using Transport.Application.Interfaces;

namespace Transport.Web.Services;

public class NbsKursService : INbsKursService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration     _config;
    private readonly IMemoryCache       _cache;

    private const string Endpoint =
        "https://webservices.nbs.rs/CommunicationOfficeService1_0/ExchangeRateService.asmx";
    private const string Ns = "http://communicationoffice.nbs.rs";

    // currencyCode: 978 = EUR
    // exchangeRateListTypeID: 2 = AKTIVA
    // rateType: 1 = kupovni, 2 = srednji, 3 = prodajni
    private const int EUR_CODE    = 978;
    private const int AKTIVA      = 2;
    private const int SREDNJI     = 2;
    private const int KUPOVNI     = 1;
    private const int PRODAJNI    = 3;

    public NbsKursService(IHttpClientFactory httpFactory, IConfiguration config, IMemoryCache cache)
    {
        _httpFactory = httpFactory;
        _config      = config;
        _cache       = cache;
    }

    /// <summary>EUR srednji kurs za datum, na 4 decimale (keširan 24h).</summary>
    public async Task<decimal> GetKursAsync(DateTime datum)
    {
        var kljuc = $"kurs_{EUR_CODE}_{datum:yyyyMMdd}";
        if (_cache.TryGetValue(kljuc, out decimal cached)) return cached;

        // Kursna lista (GetExchangeRateByDate) vraća srednji kurs na 4 decimale;
        // GetExchangeRateByRateType vraća zaokruženo na 2 decimale, koristi se samo kao fallback.
        decimal kurs = 0;
        try
        {
            var lista = await UcitajListuAsync(datum);
            kurs = lista.FirstOrDefault(v => v.Oznaka == "EUR")?.SrednjiKurs ?? 0;
        }
        catch { }

        if (kurs <= 0)
            kurs = await PozoviservisAsync(EUR_CODE, datum, AKTIVA, SREDNJI);

        if (kurs > 0)
            _cache.Set(kljuc, kurs, TimeSpan.FromHours(24));
        return kurs;
    }

    /// <summary>Sve valute sa kursne liste za dati datum (keširano 24h).</summary>
    public async Task<List<KursValuteDto>> GetSveValuteAsync(DateTime datum)
    {
        var kljuc = $"kurslista_{AKTIVA}_{datum:yyyyMMdd}";
        if (_cache.TryGetValue(kljuc, out List<KursValuteDto>? cached) && cached is not null)
            return cached;

        var lista = await UcitajListuAsync(datum);
        if (lista.Count > 0)
            _cache.Set(kljuc, lista, TimeSpan.FromHours(24));
        return lista;
    }

    // Vraća i poruku greške za dijagnostiku
    public async Task<(List<KursValuteDto> Lista, string? Greska)> GetSveValuteSaGreskamAsync(DateTime datum)
    {
        var kljuc = $"kurslista_{AKTIVA}_{datum:yyyyMMdd}";
        if (_cache.TryGetValue(kljuc, out List<KursValuteDto>? cached) && cached is not null)
            return (cached, null);

        try
        {
            var soap   = GradijSoapLista(datum);
            var client = _httpFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(15);

            var content = new StringContent(soap, Encoding.UTF8, "text/xml");
            content.Headers.Add("SOAPAction", $"\"{Ns}/GetExchangeRateByDate\"");

            var response = await client.PostAsync(Endpoint, content);
            var xml      = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return ([], $"HTTP {(int)response.StatusCode}: {xml[..Math.Min(300, xml.Length)]}");

            var lista = ParsirajListu(xml);

            if (lista.Count == 0)
            {
                // Provjeri SOAP fault
                try
                {
                    var xdoc  = XDocument.Parse(xml);
                    var fault = xdoc.Descendants()
                                    .FirstOrDefault(e => e.Name.LocalName == "faultstring")?.Value;
                    if (!string.IsNullOrEmpty(fault))
                        return ([], $"NBS greška: {fault}");

                    // Pokaži snippet XML-a da vidimo strukturu
                    var snippet = xml.Length > 600 ? xml[..600] + "..." : xml;
                    return ([], $"Prazan rezultat. XML odgovor: {snippet}");
                }
                catch
                {
                    return ([], $"Prazan rezultat. Raw: {xml[..Math.Min(400, xml.Length)]}");
                }
            }

            _cache.Set(kljuc, lista, TimeSpan.FromHours(24));
            return (lista, null);
        }
        catch (TaskCanceledException)
        {
            return ([], "Timeout — NBS ne odgovara.");
        }
        catch (Exception ex)
        {
            return ([], $"Greška: {ex.Message}");
        }
    }

    private async Task<List<KursValuteDto>> UcitajListuAsync(DateTime datum)
    {
        var (lista, _) = await GetSveValuteSaGreskamAsync(datum);
        return lista;
    }

    private string GradijSoapLista(DateTime datum)
    {
        var user = _config["ApiKeys:NBS:UserName"]  ?? string.Empty;
        var pass = _config["ApiKeys:NBS:Password"]  ?? string.Empty;
        var guid = _config["ApiKeys:NBS:LicenceID"] ?? string.Empty;
        var date = datum.ToString("yyyyMMdd");

        return
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
            "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" " +
                           "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" " +
                           "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">" +
              "<soap:Header>" +
                $"<AuthenticationHeader xmlns=\"{Ns}\">" +
                  $"<UserName>{user}</UserName>" +
                  $"<Password>{pass}</Password>" +
                  $"<LicenceID>{guid}</LicenceID>" +
                "</AuthenticationHeader>" +
              "</soap:Header>" +
              "<soap:Body>" +
                $"<GetExchangeRateByDate xmlns=\"{Ns}\">" +
                  $"<date>{date}</date>" +
                  $"<exchangeRateListTypeID>{AKTIVA}</exchangeRateListTypeID>" +
                "</GetExchangeRateByDate>" +
              "</soap:Body>" +
            "</soap:Envelope>";
    }

    private static List<KursValuteDto> ParsirajListu(string soapXml)
    {
        var result = new List<KursValuteDto>();
        try
        {
            var soapDoc = XDocument.Parse(soapXml);

            // GetExchangeRateByDateResult sadrži DIREKTNO ugniježđene XML elemente
            // (ne encoded string) — tražimo ExchangeRate direktno u SOAP dokumentu
            string? Get(XElement el, string name) =>
                el.Elements().FirstOrDefault(e => e.Name.LocalName == name)?.Value;

            decimal Dec(XElement el, string name)
            {
                var v = Get(el, name);
                if (string.IsNullOrWhiteSpace(v)) return 0;
                return decimal.TryParse(v.Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var d) ? d : 0;
            }

            foreach (var el in soapDoc.Descendants()
                                      .Where(e => e.Name.LocalName == "ExchangeRate"))
            {
                result.Add(new KursValuteDto
                {
                    Oznaka      = Get(el, "CurrencyCodeAlfaChar"),
                    NazivValute = Get(el, "CurrencyNameSerLat")
                               ?? Get(el, "CurrencyNameSerCyrl"),
                    KupovniKurs  = Dec(el, "BuyingRate"),
                    SrednjiKurs  = Dec(el, "MiddleRate"),
                    ProdajniKurs = Dec(el, "SellingRate"),
                });
            }
        }
        catch { }
        return result;
    }

    // ── SOAP poziv ────────────────────────────────────────────────────
    private async Task<decimal> PozoviservisAsync(
        int currencyCode, DateTime datum, int listaType, int rateType)
    {
        try
        {
            var soap   = GradijSOAP(currencyCode, datum, listaType, rateType);
            var client = _httpFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(15);

            var content = new StringContent(soap, Encoding.UTF8, "text/xml");
            content.Headers.Add("SOAPAction",
                $"\"{Ns}/GetExchangeRateByRateType\"");

            var response = await client.PostAsync(Endpoint, content);
            if (!response.IsSuccessStatusCode) return 0;

            var xml = await response.Content.ReadAsStringAsync();
            return ParsirajKurs(xml);
        }
        catch
        {
            return 0;
        }
    }

    private string GradijSOAP(int currencyCode, DateTime datum, int listaType, int rateType)
    {
        var user  = _config["ApiKeys:NBS:UserName"]  ?? string.Empty;
        var pass  = _config["ApiKeys:NBS:Password"]  ?? string.Empty;
        var guid  = _config["ApiKeys:NBS:LicenceID"] ?? string.Empty;
        var date  = datum.ToString("yyyyMMdd");

        return
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
            "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" " +
                           "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" " +
                           "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">" +
              "<soap:Header>" +
                $"<AuthenticationHeader xmlns=\"{Ns}\">" +
                  $"<UserName>{user}</UserName>" +
                  $"<Password>{pass}</Password>" +
                  $"<LicenceID>{guid}</LicenceID>" +
                "</AuthenticationHeader>" +
              "</soap:Header>" +
              "<soap:Body>" +
                $"<GetExchangeRateByRateType xmlns=\"{Ns}\">" +
                  $"<currencyCode>{currencyCode}</currencyCode>" +
                  $"<date>{date}</date>" +
                  $"<exchangeRateListTypeID>{listaType}</exchangeRateListTypeID>" +
                  $"<rateType>{rateType}</rateType>" +
                "</GetExchangeRateByRateType>" +
              "</soap:Body>" +
            "</soap:Envelope>";
    }

    private static decimal ParsirajKurs(string soapXml)
    {
        try
        {
            var xdoc = XDocument.Parse(soapXml);

            var result = xdoc.Descendants()
                             .FirstOrDefault(e => e.Name.LocalName == "GetExchangeRateByRateTypeResult")
                             ?.Value;

            if (string.IsNullOrWhiteSpace(result)) return 0;

            // NBS vraća decimal sa zarezom ili tačkom
            var normalized = result.Replace(',', '.');
            return decimal.TryParse(normalized,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var kurs) ? kurs : 0;
        }
        catch { return 0; }
    }
}
