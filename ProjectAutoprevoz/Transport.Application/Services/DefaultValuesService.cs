using Microsoft.EntityFrameworkCore;
using Transport.Application.Interfaces;
using Transport.Domain.Entities;
using Transport.Infrastructure.Data;

namespace Transport.Application.Services;

public class DefaultValuesService : IDefaultValuesService
{
    private readonly TransportDbContext _db;

    public DefaultValuesService(TransportDbContext db) => _db = db;

    public async Task<string?> GetDefault(string formName, string controlName, int userId)
    {
        if (userId == 0) return null;

        return await _db.DefaultValues
            .AsNoTracking()
            .Where(d => d.FormName == formName && d.ControlName == controlName && d.UserId == userId)
            .Select(d => d.ValueText)
            .FirstOrDefaultAsync();
    }

    public async Task SaveDefault(string formName, string controlName, string value, int userId)
    {
        if (userId == 0) return;

        var rec = await _db.DefaultValues
            .Where(d => d.FormName == formName && d.ControlName == controlName && d.UserId == userId)
            .FirstOrDefaultAsync();

        if (rec is null)
        {
            _db.DefaultValues.Add(new DefaultValue
            {
                FormName    = formName,
                ControlName = controlName,
                ValueText   = value,
                UserId      = userId,
                UpdatedAt   = DateTime.Now
            });
        }
        else
        {
            rec.ValueText  = value;
            rec.UpdatedAt  = DateTime.Now;
        }

        await _db.SaveChangesAsync();
    }
}
