
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
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
        /// Listar somente as empresas que o usuário recebeu pontos
        /// </summary>
        /// <param name="userId">O id, do usuário, que terá as suas empresas sendo listadas na condição delas terem recebido pontos</param>
        /// <param name="cancellationToken">Um token para o caso do solicitante cancelar a requisição</param>
        /// <returns>Uma lista de empresas que o usuário recebeu pontos</returns>
        //GET: user/{id}/company
        [HttpGet("{id}/companies/")]
        public async Task<IActionResult> GetCompaniesByUserIdAsync(int userId, CancellationToken cancellationToken)
        {

            if (!await _dbContext.Users
                    .AnyAsync(u => u.Id == userId, cancellationToken))
            {
                return NotFound("Unable to find User");
            } 
            
            var userCompanies = await _dbContext.Users
                .AsNoTracking()
                .SelectMany(u => u.Companies)
                .Select(uc => new
                {
                    Scores = uc.Score,
                    Company = uc.Company,
                    Benefits = uc.Company.Benefits
                })
                .ToListAsync(cancellationToken);

            return Ok(userCompanies);
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
                    .SingleOrDefaultAsync(u => u.Id == id,cancellationToken);

            _dbContext.Remove(dbUser);
            _dbContext.SaveChangesAsync(cancellationToken);

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
        public async Task<IActionResult> GetCompanyFromUserByIdAsync(int userId, int companyId, CancellationToken cancellationToken)
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
    }
}