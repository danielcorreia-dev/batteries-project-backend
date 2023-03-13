using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using WepApi.Interfaces;

namespace WepApi.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly BatteriesProjectDbContext _dbContext;
        public RefreshTokenService(BatteriesProjectDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<bool> DeleteRefreshTokenAsync(string Email, Guid RefreshToken, CancellationToken cancellationToken)
        {
            var dbUser = await _dbContext
                   .Users
                   .SingleOrDefaultAsync(u => u.Email == Email && u.RefreshToken == RefreshToken, cancellationToken);

            if (dbUser != null)
            {
                dbUser.RefreshToken = Guid.Empty;
                return true;
            }

            return false;
        }

        public Guid GenerateRefreshToken()
        {
            Guid guid = Guid.NewGuid();
            return guid;
        }

        public async Task<string> GetRefreshTokenAsync(string Email, CancellationToken cancellationToken)
        {
            var dbUser = await _dbContext.Users.SingleOrDefaultAsync(u => u.Email == Email, cancellationToken);
            if (dbUser == null)
                return null;
            else
                return dbUser.RefreshToken.ToString();
        }

        public async Task<bool> IsRefreshTokenExpired(string Email, CancellationToken cancellationToken)
        {
            var dbUser = await _dbContext.Users.SingleOrDefaultAsync(u => u.Email == Email, cancellationToken);
            if (dbUser == null)
                return false;

            if (DateTime.Now >= dbUser.ExpiryTime)
            {
                return true;
            }

            return false;
        }

        public async Task SaveRefreshTokenAsync(string Email, Guid NewRefreshToken, CancellationToken cancellationToken)
        {
            var dbUser = await _dbContext.Users.SingleOrDefaultAsync(u => u.Email == Email, cancellationToken);

            // if RememberMe = true then user.ExpiryTime = 30 (days) else user.ExpiryTime = 1
            if (dbUser.RememberMe)
                dbUser.ExpiryTime = DateTime.Now.AddDays(30);
            else
                dbUser.ExpiryTime = DateTime.Now.AddDays(1);

            dbUser.RefreshToken = NewRefreshToken;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
