namespace Transport.Application.Services.Sef.Models;

/// <summary>
/// Deserializacija SEF Public API v2 odgovora (vat-recording/group) — samo polja
/// koja koristimo za upis u tbl_GroupVatRecord, ostatak se ignoriše. Deserijalizuje
/// se sa PropertyNameCaseInsensitive = true, pa velika/mala slova u JSON-u ne smetaju.
/// </summary>
public class GroupVatSefDto
{
    public long?     groupVatId         { get; set; }
    public int?      year               { get; set; }
    public string?   calculationNumber  { get; set; }
    public int?      vatPeriod          { get; set; }
    public DateTime? recordingDate      { get; set; }
    public DateTime? statusChangeDate   { get; set; }
    public int?      vatRecordingStatus { get; set; }
}
