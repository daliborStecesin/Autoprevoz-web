namespace Transport.Application.Interfaces;

public interface IDefaultValuesService
{
    Task<string?> GetDefault(string formName, string controlName, int userId);
    Task SaveDefault(string formName, string controlName, string value, int userId);
}
