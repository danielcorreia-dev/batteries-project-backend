using Domain.Entities;
using Domain.Models.Params;
using Domain.Models.Results;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WebApi.Interfaces;
using Microsoft.Extensions.Configuration;


namespace WebApi.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class AuthController: ControllerBase
    {

        private readonly BatteriesProjectDbContext _dbContext;
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IConfiguration _configuration;

        public AuthController(BatteriesProjectDbContext dbContext,
            ITokenService tokenService,
            IRefreshTokenService refreshTokenService,
            IConfiguration configuration)
        {
            _dbContext = dbContext;
            _tokenService = tokenService;
            _refreshTokenService = refreshTokenService;
            _configuration = configuration;
        }

        //POST: auth/sign-in
        [HttpPost]
        [Route("sign-in")]
        [AllowAnonymous]
        public async Task<IActionResult> Signin([FromBody] SigninRequestModel signinRequestModel, CancellationToken cancellationToken)
        {

            User dbUser = await _dbContext.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    u => u.Email == signinRequestModel.Email, cancellationToken);

            if (dbUser == null)
            {
                return NotFound();
            }

            if (!dbUser.VerifyPassword(signinRequestModel.Password))
            {
                return Unauthorized();
            }
            
            dbUser.RememberMe = signinRequestModel.RememberMe;

            _dbContext.Update(dbUser);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var access_token = _tokenService.GenerateToken(dbUser);

            var refresh_token = _refreshTokenService.GenerateRefreshToken();

            await _refreshTokenService.SaveRefreshTokenAsync(dbUser.Email, refresh_token, cancellationToken);

            var resultObj = new SigninResponseModel(dbUser.Id, access_token, refresh_token);

            return Ok(resultObj);
        }

        //POST: auth/sign-up
        [HttpPost]
        [Route("sign-up")]
        [AllowAnonymous]
        public async Task<IActionResult> Signup(SignupRequestModel _signupRequestModel, CancellationToken cancellationToken)
        {


            bool isPasswordValid = ValidatePassword(_signupRequestModel.Password);
            if (!isPasswordValid) { return BadRequest(); }


            bool userExists = await _dbContext.Users
                    .AnyAsync(
                        u => u.Email == _signupRequestModel.Email, cancellationToken);

            if (userExists)
            {
                return UnprocessableEntity("User already exists");
            }

            var newUser = new User()
            {
                Email = _signupRequestModel.Email,
                Nick = _signupRequestModel.Nick,
                Password = _signupRequestModel.Password,
            };
            await _dbContext.AddAsync(newUser, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Created("user", newUser);
        }


        [NonAction]
        private bool ValidatePassword(string password)
        {
            if (password == null)
            {
                return false;
            }
            
            /* The minimum amount of characters is 8
           
                At least:
                    - 1 special character,
                    - 1 numeric character,
                    - 1 uppercase character,
                    - 1 lowercase character
            */
            Regex regExpPassword = new(@"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[^\w\s]).{8,}$");

            var isValid = regExpPassword.IsMatch(password);

            return isValid;
        }


        //POST: auth/refresh
        [HttpPost]
        [Route("refresh")]
        [AllowAnonymous]
        private async Task<IActionResult> Refresh(RefreshModel refreshModel, CancellationToken cancellationToken)
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(refreshModel.Token);
            var email = principal.Identity.Name;

            var dbUser = await _dbContext.Users.AsNoTracking()
                    .SingleOrDefaultAsync(u => u.Email == email, cancellationToken);

            var savedRefreshToken = await _refreshTokenService.GetRefreshTokenAsync(email, cancellationToken);

            if (savedRefreshToken == null)
                return NotFound();

            if (savedRefreshToken != refreshModel.RefreshToken)
                throw new SecurityTokenException("Invalid RefreshToken");

            if (await _refreshTokenService.IsRefreshTokenExpired(email, cancellationToken))
            {
                var newAccessToken = _tokenService.GenerateToken(null, principal);

                var newRefreshToken = _refreshTokenService.GenerateRefreshToken();

                await _refreshTokenService.DeleteRefreshTokenAsync(email, Guid.Parse(savedRefreshToken), cancellationToken);

                await _refreshTokenService.SaveRefreshTokenAsync(email, newRefreshToken, cancellationToken);

                if (dbUser == null)
                    return NotFound();

                return Ok(new RefreshResponseModel(dbUser.Nick, newAccessToken, newRefreshToken));

            }

            var _newAccessToken = _tokenService.GenerateToken(null, principal);

            var response = new RefreshResponseModel(dbUser.Nick, _newAccessToken, Guid.Parse(savedRefreshToken));
            return Ok(response);
        }

    }
}
