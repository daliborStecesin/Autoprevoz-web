using Transport.Application.Services.Sef.Models;

namespace Transport.Application.Services.Sef;

public interface IEFakturaUblBuilder
{
    /// Gradi UBL XML string za minimalnu FAKTURU (samo S10/S20 kategorije).
    /// Ne dira bazu, ne šalje ništa — čist string builder.
    string Build(EFakturaUblInput input);
}
