using System;
using System.Threading;
using System.Threading.Tasks;

namespace WepApi.Interfaces
{
    public interface IRefreshTokenService
    {
        public Task<string> GetRefreshTokenAsync(string Email, CancellationToken cancellationToken);
        public Task SaveRefreshTokenAsync(string Email, Guid NewRefreshToken, CancellationToken cancellationToken);
        public Task<bool> DeleteRefreshTokenAsync(string Email, Guid RefreshToken, CancellationToken cancellationToken);
        public Guid GenerateRefreshToken();
        public Task<bool> IsRefreshTokenExpired(string Email, CancellationToken cancellationToken);
    }
}
