using Transport.Application.Interfaces;
using Transport.Infrastructure;

namespace Transport.Web.Services;

public class CurrentUserService : ICurrentUser
{
    private readonly ITenantService _tenant;
    public CurrentUserService(ITenantService tenant) => _tenant = tenant;
    public int GetIdKorisnika() => _tenant.GetIdKorisnika();
}
