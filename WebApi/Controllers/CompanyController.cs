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
        
        /// <summary>
        /// Cadastrar empresa
        /// </summary>
        /// <param name="company">a empresa a ser cadastrada</param>
        /// <param name="cancellationToken">Usado para cancelar a requisição</param>
        /// <returns>Status Code 201 (Created)</returns>
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] CompanyModel company, CancellationToken cancellationToken)
        {
            
            if (await _dbContext.Companies
                    .AnyAsync(c => c.Title == company.Title, cancellationToken))
            {
                return BadRequest("Company already exists");
            }

            var newCompany = new Company()
            {
                Title = company.Title,
                Address = company.Address,
                CreatedAt = DateTimeOffset.Now,
                Benefits = new List<CompanyBenefit>(),
                Users = new List<UserCompanyScores>()
            }; 

            await _dbContext.Companies.AddAsync(newCompany, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Created(nameof(GetByIdAsync), newCompany);

        }

        /// <summary>
        /// Deletar empresa
        /// </summary>
        /// <param name="id">O id da empresa</param>
        /// <param name="cancellationToken">Usado para cancelar a requisição</param>
        /// <returns>NoContent()</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompanyByIdAsync(int id, CancellationToken cancellationToken)
        {
            if (!await _dbContext.Companies.AnyAsync(c => c.Id == id, cancellationToken))
            {
                return NotFound("Unable to find company");
            }

            var dbCompany = await _dbContext.Companies
                .AsNoTracking()
                .SingleOrDefaultAsync(c => c.Id == id, cancellationToken);
            
            _dbContext.Companies.Remove(dbCompany);
            _dbContext.SaveChangesAsync(cancellationToken);

            return NoContent();
        }
        
        /// <summary>
        /// Desabilita o beneficio de uma empresa
        /// </summary>
        /// <param name="id">O id da empresa</param>
        /// <param name="cancellationToken">Usado para cancelar a requisição</param>
        /// <returns>Ok()</returns>
        [HttpDelete("{id}/benefits")]
        public async Task<IActionResult> DeleteByIdAsync(int id, CancellationToken cancellationToken)
        {
            //verificando se a empresa, em questão, contém benefícios
            var benefits = await _dbContext.Companies
                        .Where(c => c.Id == id)
                        .Include(c => c.Benefits)
                        .SelectMany(cb => cb.Benefits)
                        .ToListAsync(cancellationToken);

            if (benefits.Count == 0)
            {
                return NotFound("This company has no benefits");
            }

            var companyBenefits = await _dbContext.Companies
                .Where(c => c.Id == id)
                .Include(c => c.Benefits)
                .SelectMany(c => c.Benefits)
                .ToListAsync(cancellationToken);

                foreach (var companyBenefit in companyBenefits)
                {
                    companyBenefit.Disabled = false;
                }
                
            _dbContext.SaveChangesAsync(cancellationToken);

            return Ok();
        }
        
    }
}