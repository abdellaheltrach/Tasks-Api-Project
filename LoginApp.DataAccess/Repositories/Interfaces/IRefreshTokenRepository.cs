using LoginApp.DataAccess.Entities;

namespace LoginApp.DataAccess.Repositories.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetTokenByTokenStringAsync(string token);
        Task<RefreshToken?> GetByUserAndDeviceAsync(int userId, string deviceId);
        Task AddAsync(RefreshToken refreshToken);
        Task UpdateAsync(RefreshToken refreshToken);
        Task RemoveRangeAsync(IEnumerable<RefreshToken> tokens);
        Task<int> DeleteInactiveTokensAsync();
        Task<List<RefreshToken>> GetExpiredOrCanceledTokensAsync(int take = 1000);
        Task SaveChangesAsync();
    }
}
