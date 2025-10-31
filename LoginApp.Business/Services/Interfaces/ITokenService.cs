using LoginApp.DataAccess.Entities;

namespace LoginApp.Business.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(int userId, string username, string role);
        RefreshToken GenerateRefreshToken(string deviceId, string deviceName);
    }
}
