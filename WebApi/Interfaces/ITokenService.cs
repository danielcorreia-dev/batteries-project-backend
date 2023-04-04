using Domain.Entities;
using System.Runtime.InteropServices;
using System.Security.Claims;

namespace WepApi.Interfaces
{
    public interface ITokenService
    {
        public string GenerateToken([Optional] User user, [Optional] ClaimsPrincipal claimsPrincipal);
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string Token);
        
    }
}
