using LoginApp.DataAccess.Data;
using LoginApp.DataAccess.Entities;
using LoginApp.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace LoginApp.DataAccess.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _context;

        public RefreshTokenRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<RefreshToken?> GetTokenByTokenStringAsync(string token)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task<RefreshToken?> GetByUserAndDeviceAsync(int userId, string deviceId)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.UserId == userId && rt.DeviceId == deviceId);
        }

        public async Task AddAsync(RefreshToken refreshToken)
        {
            await _context.RefreshTokens.AddAsync(refreshToken);
        }

        public Task UpdateAsync(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Update(refreshToken);
            return Task.CompletedTask;
        }

        public async Task RemoveRangeAsync(IEnumerable<RefreshToken> tokens)
        {
            _context.RefreshTokens.RemoveRange(tokens);
            await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteInactiveTokensAsync()
        {
            var now = DateTime.UtcNow;
            var expiredTokens = _context.RefreshTokens
                .Where(t => t.IsCanceled || t.ExpiresDate < now);

            var count = await expiredTokens.CountAsync();

            if (count > 0)
            {
                _context.RefreshTokens.RemoveRange(expiredTokens);
                await _context.SaveChangesAsync();
            }

            return count;
        }


        public async Task<List<RefreshToken>> GetExpiredOrCanceledTokensAsync(int take = 1000)
        {
            return await _context.RefreshTokens
                .Where(rt => rt.IsCanceled || rt.IsExpired)
                .OrderBy(rt => rt.ExpiresDate)
                .Take(take)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
