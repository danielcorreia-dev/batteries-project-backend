
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Models.Params;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class UserController : ControllerBase
    {
        private readonly BatteriesProjectDbContext _dbContext;

        public UserController(BatteriesProjectDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Listar os dados de perfil do usuário
        /// </summary>
        /// <param name="id"></param>
        /// <returns>
        /// {
        /// "email": "email@batteriesProject.com",
        /// "nick": "nickname0000",
        /// "total_score": "1000"
        /// }
        /// </returns>
        //GET: user/{id}
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            var dbUser = await _dbContext.Users
                .AsNoTracking()
                .Where(u => u.Id == id)
                .Select(u => new
                {
                    Email = u.Email,
                    Nick = u.Nick,
                    TotalScore = u.Companies.Sum(uc => uc.Score)
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (dbUser == null)
            {
                return NotFound("Unable to find User");
            }

            return Ok(dbUser);
        }

        /// <summary>
        /// Listar os usuários pelo nick e email, caso nao seja passado nada como parametro, retorna todos os usuários
        /// </summary>
        /// <param name="cancellationToken">Usado para cancelar a requisição</param>
        /// <returns>Os usuarios filtradas pelo seu Nick ou Email, caso nao seja passado nada como parametro de filtro, retorna todos os usuários </returns>
        //GET: user/by-nick-or-email 
        [HttpGet("by-nick-or-email")]
        public async Task<IActionResult> GetUsersByNickOrEmailAsync(CancellationToken cancellationToken)
        {

            if (!await _dbContext.Users.AnyAsync(cancellationToken))
            {
                return NotFound("Unable to find Users");
            }

            HttpContext.Request
                .Query
                .TryGetValue("Nick", out var nick);

            HttpContext.Request
                .Query
                .TryGetValue("Email", out var email);

            if (string.IsNullOrEmpty(nick) && string.IsNullOrEmpty(email))
            {
                return Ok(await _dbContext.Users
                    .AsNoTracking()
                    .ToListAsync(cancellationToken));
            }

            var dbUsers = await _dbContext.Users
                .AsNoTracking()
                .Where(u =>
                    (string.IsNullOrEmpty(nick) || u.Nick.ToLower().Contains(nick.ToString().ToLower())) &&
                    (string.IsNullOrEmpty(email) || u.Email.ToLower().Contains(email.ToString().ToLower()))
                )
                .ToListAsync(cancellationToken);

            return Ok(dbUsers);
        }

        /// <summary>
        /// Listar todos os usuários
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAllUsersAsync(CancellationToken cancellationToken)
        {
            if (!await _dbContext.Users.AnyAsync(cancellationToken))
            {
                return NotFound("Unable to find Users");
            }

            var dbUsers = await _dbContext.Users
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return Ok(dbUsers);
        }

        /// <summary>
        /// Deletar usuário pelo Id
        /// </summary>
        /// <param name="id">O id, do usuário, a ser deletado</param>
        /// <param name="cancellationToken">Um token para o caso do solicitante cancelar a requisição</param>
        /// <returns>Status Code 204 (NoContent)</returns>
        //DELETE: user/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
        {

            if (!await _dbContext.Users
                    .AnyAsync(u => u.Id == id, cancellationToken))
            {
                return NotFound("Unable to find User");
            }

            var dbUser = await _dbContext.Users
                .SingleOrDefaultAsync(u => u.Id == id, cancellationToken);

            _dbContext.Remove(dbUser);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return NoContent();
        }

        /// <summary>
        /// Logar como empresa
        /// </summary>
        /// <param name="userId">O id do usuário</param>
        /// <param name="companyId">O id da empresa</param>
        /// <param name="cancellationToken">Um token para o caso do solicitante cancelar a requisição</param>
        /// <returns>Status Code 200 {"Id": {id}, "Title":"someTitle", "Address":"someAddress"}</returns>
        //GET: user/{id}/company/{id}
        [HttpGet("{userId}/company/{companyId}")]
        public async Task<IActionResult> GetCompanyFromUserByIdAsync(int userId, int companyId,
            CancellationToken cancellationToken)
        {

            if (!await _dbContext.Users.AnyAsync(u => u.Id == userId, cancellationToken))
            {
                return NotFound("Unable to find user, cannot to log in");
            }

            if (!await _dbContext.Users
                    .AsNoTracking()
                    .Where(u => u.Id == userId)
                    .SelectMany(u => u.Companies)
                    .AnyAsync(uc => uc.CompanyId == companyId, cancellationToken))
            {
                return NotFound("Unable to find company, cannot to log in");
            }


            var dbCompany = await _dbContext.Users
                .Where(u => u.Id == userId)
                .SelectMany(u => u.Companies)
                .Where(uc => uc.CompanyId == companyId)
                .Select(uc => new
                {
                    Id = uc.CompanyId,
                    Title = uc.Company.Title,
                    Address = uc.Company.Address
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return Ok(dbCompany);

        }

        /// <summary>
        /// Retornar todas as empresas que o usuário recebeu pontos
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        //GET: user/{id}/companies/with-points
        [HttpGet("{id}/companies/with-points")]
        public async Task<IActionResult> GetUserCompaniesWithPointsAsync(int id, CancellationToken cancellationToken)
        {
            if (!await _dbContext.Users
                    .AnyAsync(u => u.Id == id, cancellationToken))
            {
                return NotFound("Unable to find User");
            }

            var userCompaniesWithPoints = await _dbContext.Users
                .AsNoTracking()
                .Where(u => u.Id == id)
                .SelectMany(u => u.Companies)
                .Where(ucs => ucs.Score > 0)
                .Select(uc => new
                {
                    Scores = uc.Score,
                    Company = uc.Company,
                    Benefits = uc.Company.Benefits
                })
                .ToListAsync(cancellationToken);

            return userCompaniesWithPoints.Count > 0
                ? Ok(userCompaniesWithPoints)
                : NotFound("User has no companies with points");
        }

        /// <summary>
        /// Recuperar empresa que o usuário é dono
        /// </summary>
        /// <param name="id">O id do usuario</param>
        /// <param name="cancellationToken">Um token para o caso do solicitante cancelar a requisição</param>
        /// <returns>A empresa que o usuário é dono</returns>
        ///GET: user/{id}/company
        [HttpGet("{id}/company")]
        public async Task<IActionResult> GetCompanyAsync(int id, CancellationToken cancellationToken)
        {
            if (!await _dbContext.Users
                    .AnyAsync(u => u.Id == id, cancellationToken))
            {
                return NotFound("Unable to find User");
            }

            var userCompany = await _dbContext.Companies
                 .AsNoTracking()
                 .Where(c => c.UserId == id)
                 .Select(c => new Company 
                 {
                     Title = c.Title,
                     Address = c.Address,
                     OpeningHours = c.OpeningHours,
                     PhoneNumber = c.PhoneNumber,
                     UserId = c.UserId,
                 })
                 .SingleOrDefaultAsync(cancellationToken);
            
            if(userCompany == null)
            {
                return NotFound("User has no company");
            }
            
            return Ok(userCompany);

        }
    }
}