using System.Text;
using System.Xml.Linq;
using Transport.Application.DTOs;
using Transport.Application.Interfaces;
using Transport.Domain.Helpers;

namespace Transport.Web.Services;

public class NbsPartnerService : INbsPartnerService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration     _config;

    // XML servis — vraća GetCompanyAccountResult kao string (XML sadržaj)
    private const string Endpoint =
        "https://webservices.nbs.rs/CommunicationOfficeService1_0/CompanyAccountXmlService.asmx";
    private const string Ns = "http://communicationoffice.nbs.rs";

    public NbsPartnerService(IHttpClientFactory httpFactory, IConfiguration config)
    {
        _httpFactory = httpFactory;
        _config      = config;
    }

    public async Task<(List<NbsKompanija> Rezultati, string? Greska)> PretraziSaDetaljimaAsync(
        long? mb = null, string? pib = null, string? naziv = null)
    {
        try
        {
            var soap   = BuildSoapRequest(mb, pib, naziv);
            var client = _httpFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(30);

            var content = new StringContent(soap, Encoding.UTF8, "text/xml");
            content.Headers.Add("SOAPAction", $"\"{Ns}/GetCompanyAccount\"");

            var response = await client.PostAsync(Endpoint, content);
            var xml      = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return ([], $"HTTP {(int)response.StatusCode}: {xml[..Math.Min(400, xml.Length)]}");

            var rezultati = ParseSoapResponse(xml, out var greska);
            return (rezultati, greska);
        }
        catch (TaskCanceledException)
        {
            return ([], "Veza sa NBS istekla (30s timeout).");
        }
        catch (Exception ex)
        {
            return ([], $"Greška: {ex.Message}");
        }
    }

    public async Task<List<NbsKompanija>> PretraziAsync(
        long? mb = null, string? pib = null, string? naziv = null)
    {
        var (rezultati, _) = await PretraziSaDetaljimaAsync(mb, pib, naziv);
        return rezultati;
    }

    // Parametri prema Reference.cs (PartneriNBSxml):
    // nationalIdentificationNumber, taxIdentificationNumber, bankCode,
    // accountNumber, controlNumber, companyName, city, startItemNumber, endItemNumber
    private string BuildSoapRequest(long? mb, string? pib, string? naziv)
    {
        var user  = _config["ApiKeys:NBS:UserName"]  ?? string.Empty;
        var pass  = _config["ApiKeys:NBS:Password"]  ?? string.Empty;
        var guid  = _config["ApiKeys:NBS:LicenceID"] ?? string.Empty;

        // Nullable long/int → xsi:nil="true" ako nije postavljeno
        string Nil(string name) => $"<{name} xsi:nil=\"true\"/>";
        string LongElem(string name, long? val) => val.HasValue
            ? $"<{name}>{val.Value}</{name}>"
            : Nil(name);

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
                $"<GetCompanyAccount xmlns=\"{Ns}\">" +
                  LongElem("nationalIdentificationNumber", mb) +
                  $"<taxIdentificationNumber>{pib ?? string.Empty}</taxIdentificationNumber>" +
                  Nil("bankCode") +
                  Nil("accountNumber") +
                  Nil("controlNumber") +
                  $"<companyName>{naziv ?? string.Empty}</companyName>" +
                  "<city></city>" +
                  Nil("startItemNumber") +
                  Nil("endItemNumber") +
                "</GetCompanyAccount>" +
              "</soap:Body>" +
            "</soap:Envelope>";
    }

    private static List<NbsKompanija> ParseSoapResponse(string soapXml, out string? greska)
    {
        greska = null;
        var result = new List<NbsKompanija>();
        try
        {
            var soapDoc = XDocument.Parse(soapXml);

            // Provjeri SOAP fault
            var fault = soapDoc.Descendants()
                               .FirstOrDefault(e => e.Name.LocalName == "faultstring")?.Value;
            if (!string.IsNullOrEmpty(fault))
            {
                greska = $"NBS: {fault}";
                return result;
            }

            // Izvuci GetCompanyAccountResult — sadrži XML string kao vrijednost
            var resultNode = soapDoc.Descendants()
                                    .FirstOrDefault(e => e.Name.LocalName == "GetCompanyAccountResult");
            if (resultNode == null)
            {
                greska = "Nema GetCompanyAccountResult u odgovoru.";
                return result;
            }

            // Value automatski dekodira HTML entitete (&lt; → <)
            var xmlContent = resultNode.Value;
            if (string.IsNullOrWhiteSpace(xmlContent))
                return result;

            // Parsiraj inner XML — sadrži CompanyAccount elemente
            var innerDoc = XDocument.Parse(xmlContent);

            string? Get(XElement el, string name) =>
                el.Elements()
                  .FirstOrDefault(e => e.Name.LocalName == name)?.Value;

            foreach (var el in innerDoc.Descendants()
                                       .Where(e => e.Name.LocalName == "CompanyAccount"))
            {
                var pib = Get(el, "TaxIdentificationNumber");
                var mb  = Get(el, "NationalIdentificationNumber");

                // Ako matični broj ima 7 cifara → dodaj vodeću nulu
                if (!string.IsNullOrEmpty(mb) && mb.Length == 7)
                    mb = "0" + mb;

                var kompanija = result.FirstOrDefault(k =>
                    (!string.IsNullOrEmpty(pib) && k.Pib == pib) ||
                    (!string.IsNullOrEmpty(mb)  && k.MaticniBroj == mb));

                if (kompanija == null)
                {
                    kompanija = new NbsKompanija
                    {
                        Naziv       = Get(el, "CompanyName"),
                        Pib         = pib,
                        MaticniBroj = mb,
                        Adresa      = Get(el, "Address"),
                        Mesto       = Get(el, "City"),
                    };
                    result.Add(kompanija);
                }

                // NBS vraća žiro račun u 3 dijela: BankCode + AccountNumber + ControlNumber
                // Format: XXX-XXXXXXXXXXXXX-XX (šifra banke - 13 cifara - kontrolni)
                var sifraBanke    = Get(el, "BankCode")       ?? Get(el, "BankID");
                var brojSrednji   = Get(el, "AccountNumber");
                var kontrolni     = Get(el, "ControlNumber")  ?? Get(el, "CheckDigit");
                var banka         = Get(el, "BankName")       ?? Get(el, "Bank");

                var ziroRacun = ZiroRacunHelper.Formatiraj(sifraBanke, brojSrednji, kontrolni);

                if (!string.IsNullOrWhiteSpace(ziroRacun) &&
                    !kompanija.Racuni.Any(r => r.ZiroRacun == ziroRacun))
                {
                    kompanija.Racuni.Add(new NbsRacun { ZiroRacun = ziroRacun, Banka = banka });
                }
            }
        }
        catch (Exception ex)
        {
            greska = $"Greška parsiranja: {ex.Message}";
        }
        return result;
    }
}
