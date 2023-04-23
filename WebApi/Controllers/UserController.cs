
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure;
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
                    TotalScore = u.Companies.Sum(uc => uc.Scores)
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

            var userCompanies = await _dbContext.Users
                .AsNoTracking()
                .SelectMany(u => u.Companies)
                .Select(uc => new
                {
                    scores = uc.Scores,
                    company = uc.Company,
                    benefits = uc.Company.Benefits
                })
                .ToListAsync(cancellationToken);

            return Ok(userCompanies);
        }

    }
}