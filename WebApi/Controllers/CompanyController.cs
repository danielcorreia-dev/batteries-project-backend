using System;
using System.Collections.Generic;
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
    public class CompanyController: ControllerBase
    {

        private readonly BatteriesProjectDbContext _dbContext;
        
        public CompanyController(BatteriesProjectDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Listar os dados de perfil da empresa
        /// </summary>
        /// <param name="id">O id da empresa</param>
        /// <param name="cancellationToken">Usado para cancelar a requisição</param>
        /// <returns>Ok({id, titulo, endereco})</returns>
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            if (!await _dbContext.Companies
                        .AnyAsync(c => c.Id == id, cancellationToken))
            {
                return NotFound("Unable to find company");
            }

            var companyProfileData = await _dbContext.Companies
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    Id = c.Id,
                    Title = c.Title,
                    Address = c.Address
                })
                .ToListAsync(cancellationToken);

            return Ok(companyProfileData);
        }
        
        
        
    }
}