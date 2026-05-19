namespace Transport.Application.DTOs;

public class KursValuteDto
{
    public string?  NazivValute  { get; set; }
    public string?  Oznaka       { get; set; }
    public decimal  KupovniKurs  { get; set; }
    public decimal  SrednjiKurs  { get; set; }
    public decimal  ProdajniKurs { get; set; }
}
