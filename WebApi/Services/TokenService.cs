using Domain.Entities;
using Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using WebApi.Interfaces;

namespace WebApi.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration; 
        private readonly IBatteriesProjectDbContext _dbContext;
        public TokenService(IConfiguration configuration, IBatteriesProjectDbContext dbContext)
        {
            _configuration = configuration;
            _dbContext = dbContext;
        }

        private readonly JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

        private SecurityTokenDescriptor tokenDescriptor;

        public string GenerateToken([Optional] User user, [Optional] ClaimsPrincipal claimsPrincipal)
        {

            var key = Encoding.ASCII.GetBytes(_configuration["SecretyKey"]);

            //Generating the object used to provide as parameter for 
            //JwtSecurityTokenHandler's CreateToken method

            //generate token body for the first time
            if (user != null)
            {

                var claimsList = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.Nick),
                };

                var identity = new ClaimsIdentity(claimsList);

                tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(identity),
                    Expires = DateTime.UtcNow.AddMinutes(30),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
            }
            //Update the token body with a new Identity which it contains Claims 
            else
            {
                tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claimsPrincipal.Claims),
                    Expires = DateTime.UtcNow.AddMinutes(30),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
            }

            //generating the token
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // Encrypts the token and then it is returned
            return tokenHandler.WriteToken(token);
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string Token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue("string", "SecretyKey"))), //    private key/ secret key
                ValidateLifetime = false,
            };

            var principal = tokenHandler.ValidateToken(Token, tokenValidationParameters, out var securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }
    }
}
