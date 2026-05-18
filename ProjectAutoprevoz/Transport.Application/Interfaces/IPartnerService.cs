using Transport.Domain.Entities;

namespace Transport.Application.Interfaces;

/// <summary>
/// Servis za upravljanje partnerima
/// </summary>
public interface IPartnerService
{
    Task<List<Partner>> GetSviPartneriAsync();
    Task<Partner?> GetPartnerByIdAsync(int broj);
    Task<Partner> CreatePartnerAsync(Partner partner);
    Task<Partner> UpdatePartnerAsync(Partner partner);
    Task DeletePartnerAsync(int broj);
    Task<bool> CheckPibUniqueAsync(string pib, int? excludeId = null);
}
