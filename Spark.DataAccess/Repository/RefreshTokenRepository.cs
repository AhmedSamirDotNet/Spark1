using Microsoft.EntityFrameworkCore;
using Spark.DataAccess.Data;
using Spark.DataAccess.Repository.IRepository;
using Spark.Models;
using System;
using System.Threading.Tasks;

namespace Spark.DataAccess.Repository
{
    public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
    {
        private readonly ApplicationDbContext _db;

        public RefreshTokenRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        // يجيب RefreshToken صالح (مش موقوف ولا منتهي)
        public async Task<RefreshToken?> GetValidRefreshTokenAsync(string token)
        {
            return await _db.RefreshTokens
                .Include(r => r.AdminUser)
                .FirstOrDefaultAsync(r =>
                    r.Token == token &&
                    r.RevokedAt == null &&
                    r.ExpiresAt > DateTime.UtcNow);
        }

        // يوقف RefreshToken
        public async Task RevokeRefreshTokenAsync(string token)
        {
            var refreshToken = await _db.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == token);

            if (refreshToken != null && refreshToken.RevokedAt == null)
            {
                refreshToken.RevokedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
        }
    }
}
