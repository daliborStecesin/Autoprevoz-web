using Transport.Application.Services.Sef.Models;
using Transport.Domain.Entities;

namespace Transport.Application.Services.Sef;

public interface IObavestenjaPPService
{
    /// Povlači primljena obaveštenja sa SEF-a (recipient/date-range) i upisuje nova u lokalnu bazu (dedupe po noticeId).
    Task<int> ImportPrimljenaAsync(DateTime datumOd, DateTime datumDo);

    /// Povlači poslata obaveštenja sa SEF-a (sender/date-range) i upisuje nova u lokalnu bazu (dedupe po noticeId).
    Task<int> ImportPoslataAsync(DateTime datumOd, DateTime datumDo);

    /// Čitanje iz lokalne baze, sortirano NoticeDate DESC.
    Task<List<ObavestenjePP>> GetListaAsync(string tipSender, string? posiljalacFilter, string? brojFilter, DateTime? datumOd, DateTime? datumDo);

    /// Mapira UI vrednosti u SEF API format, validira kombinaciju, šalje POST sender/send,
    /// pri uspehu inkrementira Broj_Otpis i upisuje rezultat u lokalnu bazu.
    Task<ObavestenjePP> PosaljiAsync(ObavestenjePPSendDto podaci, string uiOsnov, string uiTipReference, string uiPoreklo);

    Task<byte[]> PreuzmiPdfAsync(long noticeId, bool isSender);

    /// Sledeći broj u O-X-YYYY formatu, BEZ increment-a brojača.
    Task<string> GeneriseBrojAsync();
}
