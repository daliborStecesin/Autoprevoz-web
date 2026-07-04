using Microsoft.EntityFrameworkCore;
using Transport.Application.Services.Sef.Models;
using Transport.Domain.Entities;
using Transport.Infrastructure.Data;

namespace Transport.Application.Services.Sef;

/// <summary>
/// Evidencija prethodnog poreza (recipients-notice-on-input-vat) — prevod desktop
/// Class_ObavestenjaPP.cs logike na SefApiClient/EF Core. Isti pattern kao SefService:
/// sefApiKey/sefTipServera se čitaju iz tbl_Podesavanja pre svakog poziva.
/// </summary>
public class ObavestenjaPPService : IObavestenjaPPService
{
    private readonly SefApiClient      _api;
    private readonly TransportDbContext _db;

    public ObavestenjaPPService(SefApiClient api, TransportDbContext db)
    {
        _api = api;
        _db  = db;
    }

    private async Task<(string apiKey, string tipServera)> GetSettings()
    {
        var pod = await _db.Podesavanja.AsNoTracking().FirstOrDefaultAsync();
        return (pod?.sefApiKey ?? string.Empty, pod?.sefTipServera ?? "DEMO");
    }

    // ── Broj dokumenta (O-{Broj_Otpis}-{godina}) ─────────────────────────────
    public async Task<string> GeneriseBrojAsync()
    {
        var pod  = await _db.Podesavanja.AsNoTracking().FirstOrDefaultAsync();
        var broj = pod?.Broj_Otpis ?? 0;
        return $"O-{broj}-{DateTime.Now.Year}";
    }

    // ── Import sa SEF-a (GET, date-range) ────────────────────────────────────
    public Task<int> ImportPrimljenaAsync(DateTime datumOd, DateTime datumDo)
        => ImportAsync(datumOd, datumDo, poslata: false);

    public Task<int> ImportPoslataAsync(DateTime datumOd, DateTime datumDo)
        => ImportAsync(datumOd, datumDo, poslata: true);

    private async Task<int> ImportAsync(DateTime datumOd, DateTime datumDo, bool poslata)
    {
        var (apiKey, tipServera) = await GetSettings();

        var smer     = poslata ? "sender" : "recipient";
        var endpoint = $"recipients-notice-on-input-vat/{smer}/date-range" +
                       $"?dateFrom={datumOd:yyyy-MM-dd}&dateTo={datumDo:yyyy-MM-dd}";

        var lista = await _api.GetAsync<List<ObavestenjePPResponseDto>>(apiKey, tipServera, endpoint);
        if (lista is null or { Count: 0 }) return 0;

        var tipSender = poslata ? "POSLATA" : "PRIMLJENA";
        var senderId  = poslata ? 1 : 0;

        // Napomena: dedupe se radi u memoriji (ne preko .Contains() nad parametrizovanom
        // listom u EF upitu) — EF Core 8 prevodi takav .Contains() u OPENJSON(...) WITH (...),
        // što ovaj SQL Server (niži compatibility level) ne podržava ("Incorrect syntax near 'WITH'").
        var postojeciIds = (await _db.ObavestenjaPP
                .Where(x => x.noticeId.HasValue)
                .Select(x => x.noticeId!.Value)
                .ToListAsync())
            .ToHashSet();

        int upisano = 0;
        foreach (var n in lista)
        {
            if (n.Id == 0) continue;
            if (!postojeciIds.Add(n.Id)) continue; // dedupe po noticeId (i unutar iste liste)

            // KLJUČNO: za POSLATA čita se Recipient.* (kome smo poslali),
            // za PRIMLJENA čita se Sender.* (ko nam je poslao) — isto kao stari UpisiNoticeU_Bazu
            var druga = poslata ? n.Recipient : n.Sender;

            _db.ObavestenjaPP.Add(new ObavestenjePP
            {
                noticeId       = n.Id,
                noticeNumber   = n.NoticeNumber ?? "",
                NoticeDate     = n.NoticeDate ?? DateTime.Now,
                recipientPIB   = druga?.VatRegistrationCode ?? "",
                recipientMB    = druga?.RegistrationCode ?? "",
                totalVatAmount = n.TotalVatAmount ?? 0,
                Sender         = druga?.Name ?? "",
                tipSender      = tipSender,
                documentNumber = n.RelatedDocumentNumber ?? "",
                statust        = PrevediStatus(n.SendingStatus ?? ""),
                senderId       = senderId
            });
            upisano++;
        }

        await _db.SaveChangesAsync();
        return upisano;
    }

    // ── Čitanje iz lokalne baze ───────────────────────────────────────────────
    public async Task<List<ObavestenjePP>> GetListaAsync(
        string tipSender, string? posiljalacFilter, string? brojFilter, DateTime? datumOd, DateTime? datumDo)
    {
        var q = _db.ObavestenjaPP.AsNoTracking().Where(x => x.tipSender == tipSender);

        if (!string.IsNullOrWhiteSpace(posiljalacFilter))
            q = q.Where(x => x.Sender != null && x.Sender.Contains(posiljalacFilter));

        if (!string.IsNullOrWhiteSpace(brojFilter))
            q = q.Where(x => x.noticeNumber != null && x.noticeNumber.Contains(brojFilter));

        if (datumOd.HasValue)
            q = q.Where(x => x.NoticeDate >= datumOd.Value);

        if (datumDo.HasValue)
            q = q.Where(x => x.NoticeDate <= datumDo.Value);

        return await q.OrderByDescending(x => x.NoticeDate).ToListAsync();
    }

    // ── Slanje (POST sender/send) ────────────────────────────────────────────
    public async Task<ObavestenjePP> PosaljiAsync(
        ObavestenjePPSendDto podaci, string uiOsnov, string uiTipReference, string uiPoreklo)
    {
        var originApi = MapDocumentIssueOriginUiToApi(uiPoreklo);
        var basisApi  = MapBasisOfNoticeUiToApi(uiOsnov);
        var refApi    = MapDocumentReferenceTypeUiToApi(uiTipReference);

        ValidateNoticeCombination(basisApi, refApi);

        if (refApi == "CreditNoteReferenceToPeriod" &&
            (podaci.RelatedInvoicePeriodStartDate is null || podaci.RelatedInvoicePeriodEndDate is null))
        {
            throw new ArgumentException("Za 'Dokument o smanjenju -> period' obavezni su datumi početka i kraja perioda.");
        }

        if (basisApi == "Storno" && podaci.RelatedDocumentStornoDate is null)
        {
            throw new ArgumentException("Za 'Storno' je preporučeno/obavezno popuniti datum storniranja.");
        }

        podaci.DocumentIssueOrigin   = originApi;
        podaci.BasisOfNotice         = basisApi;
        podaci.DocumentReferenceType = refApi;
        podaci.NoticeNumber          = await GeneriseBrojAsync();

        var (apiKey, tipServera) = await GetSettings();

        ObavestenjePPResponseDto? response;
        try
        {
            response = await _api.PostAsync<ObavestenjePPResponseDto, ObavestenjePPSendDto>(
                apiKey, tipServera,
                "recipients-notice-on-input-vat/sender/send",
                podaci);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("NoticeNumberNotUnique", StringComparison.OrdinalIgnoreCase))
        {
            // Lokalni brojač (Broj_Otpis) je desinhronizovan sa SEF-om — broj koji smo predložili
            // je tamo već zauzet (npr. ranije poslat sa desktop verzije ili u prethodnom pokušaju
            // koji je uspeo na SEF-u, ali je lokalni increment ostao neizvršen). Pomeramo brojač
            // preko zaglavljenog broja da sledeći pokušaj koristi slobodan broj.
            var pod2 = await _db.Podesavanja.FirstOrDefaultAsync();
            if (pod2 is not null)
            {
                pod2.Broj_Otpis = (pod2.Broj_Otpis ?? 0) + 1;
                await _db.SaveChangesAsync();
            }

            throw new InvalidOperationException(
                $"Broj obaveštenja '{podaci.NoticeNumber}' je već iskorišćen na SEF-u. " +
                "Brojač je pomeren na sledeći broj — pokušajte ponovo.");
        }

        // Broj se uveća TEK POSLE uspešnog POST-a (isto pravilo kao ostali brojevi dokumenata)
        var pod = await _db.Podesavanja.FirstOrDefaultAsync();
        if (pod is not null)
        {
            pod.Broj_Otpis = (pod.Broj_Otpis ?? 0) + 1;
            await _db.SaveChangesAsync();
        }

        // Isto kao stari UpisiPoslatoObavestenjeIzResponse -> UpisiNoticeU_Bazu(n, "POSLATA", 1):
        // za POSLATA se čita Recipient.* iz odgovora (kome je poslato)
        var entitet = new ObavestenjePP
        {
            noticeId       = response?.Id,
            noticeNumber   = podaci.NoticeNumber,
            NoticeDate     = response?.NoticeDate ?? DateTime.Now,
            recipientPIB   = response?.Recipient?.VatRegistrationCode ?? podaci.Recipient.VatRegistrationCode,
            recipientMB    = response?.Recipient?.RegistrationCode ?? podaci.Recipient.RegistrationCode,
            totalVatAmount = podaci.TotalVatAmount,
            Sender         = response?.Recipient?.Name,
            tipSender      = "POSLATA",
            senderId       = 1,
            documentNumber = podaci.RelatedDocumentNumber,
            statust        = PrevediStatus(response?.SendingStatus ?? "")
        };

        _db.ObavestenjaPP.Add(entitet);
        await _db.SaveChangesAsync();

        return entitet;
    }

    // ── PDF ───────────────────────────────────────────────────────────────────
    public async Task<byte[]> PreuzmiPdfAsync(long noticeId, bool isSender)
    {
        var (apiKey, tipServera) = await GetSettings();
        var direction = isSender ? "sender" : "recipient";
        var endpoint  = $"recipients-notice-on-input-vat/{direction}/{noticeId}/pdf?noticeId={noticeId}";
        return await _api.GetBytesAsync(apiKey, tipServera, endpoint);
    }

    // ── Prevodi (SR UI -> API) — 1:1 prevod Class_ObavestenjaPP.cs ───────────
    private static string MapDocumentIssueOriginUiToApi(string uiValue)
    {
        if (string.IsNullOrWhiteSpace(uiValue)) return "DocumentIssuedViaSef";
        uiValue = uiValue.Trim();

        switch (uiValue)
        {
            case "Dokument izdat preko SEF-a":
            case "Preko SEF-a":
            case "SEF":
                return "DocumentIssuedViaSef";

            case "Dokument izdat van SEF-a":
            case "Van SEF-a":
            case "VAN SEF-a":
                return "DocumentIssuedOutsideSef";
        }

        if (uiValue.StartsWith("DocumentIssued")) return uiValue;
        return "DocumentIssuedViaSef";
    }

    private static string MapBasisOfNoticeUiToApi(string uiValue)
    {
        if (string.IsNullOrWhiteSpace(uiValue)) return "CreditNoteIssued";
        uiValue = uiValue.Trim();

        switch (uiValue)
        {
            case "Dokument o smanjenju":
            case "Kreditni dokument":
            case "Dok. o smanjenju":
                return "CreditNoteIssued";

            case "Storno":
            case "Storniranje":
                return "Storno";
        }

        if (uiValue == "CreditNoteIssued" || uiValue == "Storno") return uiValue;
        return "CreditNoteIssued";
    }

    private static string MapDocumentReferenceTypeUiToApi(string uiValue)
    {
        if (string.IsNullOrWhiteSpace(uiValue)) return "CreditNoteReferenceToInvoice";
        uiValue = uiValue.Trim();

        switch (uiValue)
        {
            case "Dokument o smanjenju koji se odnosi na fakturu za promet":
                return "CreditNoteReferenceToInvoice";

            case "Dokument o smanjenju koji se odnosi na avansnu fakturu":
                return "CreditNoteReferenceToPrepaymentInvoice";

            case "Dokument o smanjenju koji se odnosi na vremenski period":
                return "CreditNoteReferenceToPeriod";

            case "Storno fakture za promet":
                return "StornoInvoice";

            case "Storno avansne fakture":
                return "StornoPrepayment";

            case "Storno dokumenta o povecanju":
                return "StornoDebitNote";
        }

        return uiValue;
    }

    private static void ValidateNoticeCombination(string basisOfNoticeApi, string documentReferenceTypeApi)
    {
        bool ok;

        if (basisOfNoticeApi == "CreditNoteIssued")
        {
            ok = documentReferenceTypeApi == "CreditNoteReferenceToInvoice"
                 || documentReferenceTypeApi == "CreditNoteReferenceToPrepaymentInvoice"
                 || documentReferenceTypeApi == "CreditNoteReferenceToPeriod";
        }
        else if (basisOfNoticeApi == "Storno")
        {
            ok = documentReferenceTypeApi == "StornoInvoice"
                 || documentReferenceTypeApi == "StornoPrepayment"
                 || documentReferenceTypeApi == "StornoDebitNote";
        }
        else
        {
            ok = false;
        }

        if (!ok)
            throw new ArgumentException($"Nedozvoljena kombinacija: Osnov='{basisOfNoticeApi}', Referenca='{documentReferenceTypeApi}'.");
    }

    // ── Status ────────────────────────────────────────────────────────────────
    public static string PrevediStatus(string apiStatus)
    {
        if (string.IsNullOrWhiteSpace(apiStatus))
            return "";

        return apiStatus.Trim() switch
        {
            "Draft"    => "Nacrt",
            "Sent"     => "Poslato",
            "Mistake"  => "Greška",
            "Accepted" => "Prihvaćeno",
            "Rejected" => "Odbijeno",
            "Canceled" => "Poništeno",
            "Received" => "Primljeno",
            _          => apiStatus
        };
    }
}
