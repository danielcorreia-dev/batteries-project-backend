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
        
        [HttpGet]
        public async Task<IActionResult> GetAllCompaniesAsync(CancellationToken cancellationToken)
        {
            var companies = await _dbContext.Companies
                .AsNoTracking()
                .Select(c => new
                {
                    Id = c.Id,
                    Title = c.Title,
                    Address = c.Address
                })
                .ToListAsync(cancellationToken);

            return Ok(companies);
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
                Users = new List<UserCompanyScore>()
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
            await _dbContext.SaveChangesAsync(cancellationToken);

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
                
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok();
        }

        /// <summary>
        /// Atualizar as configurações da empresa
        /// </summary>
        /// <param name="companyId">O id da empresa</param>
        /// <param name="benefitId">O id do beneficio</param>
        /// <param name="companyBenefit">o objeto companyBenefits contendo os novos beneficios</param>
        /// <param name="cancellationToken">Usado para cancelar a requisição</param>
        /// <returns>Created()</returns>
        [HttpPut("{companyId}/benefit/{benefitId}")]
        public async Task<IActionResult> PutAsync(int companyId, int benefitId, [FromBody] CompanyBenefitsModel companyBenefit, CancellationToken cancellationToken)
        {
            if (!await _dbContext.Companies
                    .AnyAsync(c => c.Id == companyId, cancellationToken))
            {
                return NotFound("Unable to find company");
            }

            if (await _dbContext.Companies
                    .Where(c => c.Id == companyId)
                    .SelectMany(c => c.Benefits)
                    .AnyAsync(cb => cb.Id == benefitId, cancellationToken))
            {
                return NotFound($"Unable to find benefit");
            }
            
            var dbBenefit = await _dbContext.Companies
                .Where(c => c.Id == companyId)
                .SelectMany(c => c.Benefits)
                .SingleOrDefaultAsync(cb => cb.Id == benefitId,cancellationToken);


            dbBenefit.Benefit = companyBenefit.Benefit;
            dbBenefit.Description = companyBenefit.Description;
            dbBenefit.ScoreNeeded = companyBenefit.ScoreNeeded;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Created(nameof(GetByIdAsync), dbBenefit);

        }

        /// <summary>
        /// Adicionar pontos ao usuário // cadastrar um novo UserCompanyScore
        /// </summary>
        /// <param name="id">o id da empresa</param>
        /// <param name="userCompanyScoreModel">O novo UserCompanyScore a ser inserido no banco</param>
        /// <param name="cancellationToken">Usado para cancelar a requisição</param>
        /// <returns>Created()</returns>
        [HttpPost("{id}/user")]
        public async Task<IActionResult> PostUserCompanyScoresAsync(int id, [FromBody] UserCompanyScoreModel userCompanyScoreModel, CancellationToken cancellationToken)
        {
            if (id != userCompanyScoreModel.CompanyId)
            {
                return BadRequest("The CompanyId of the url is different from the CompanyId of the body");
            }
            
            if (!await _dbContext.Companies
                    .AnyAsync(c => c.Id == id, cancellationToken))
            {
                return NotFound("Unable to find company");
            }
            
            if (!await _dbContext.Users
                    .AnyAsync(u => u.Id == userCompanyScoreModel.userId, cancellationToken))
            {
                return NotFound("Unable to find user");
            }

            if (await _dbContext.UserCompanyScores
                    .AnyAsync(ucs =>
                        ucs.CompanyId == id && ucs.UserId == userCompanyScoreModel.userId, cancellationToken))
            {
                return BadRequest($"CompanyId and UserId must be an unique value. " +
                                  $"The record with UserId = {userCompanyScoreModel.userId} and CompanyId = {userCompanyScoreModel.CompanyId} already exists");
            }
            
            var newUsc = new UserCompanyScore()
            {
                Score = userCompanyScoreModel.Score,
                CompanyId = userCompanyScoreModel.CompanyId,
                UserId = userCompanyScoreModel.userId
            };

            await _dbContext.UserCompanyScores.AddAsync(newUsc, cancellationToken);
            
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Created(nameof(GetByIdAsync),newUsc);
        }
        
        /// <summary>
        /// Atualiza as configurações da empresa
        /// </summary>
        /// <param name="id">O id da company a ser atualizada</param>
        /// <param name="companyModel">Os novos dados da company</param>
        /// <param name="cancellationToken">Usado para cancelar a requisição</param>
        /// <returns>Ok()</returns>
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchAsync(int id, [FromBody] CompanyModel companyModel ,CancellationToken cancellationToken)
        {
            if (!await _dbContext.Companies
                    .AnyAsync(c => c.Id == id, cancellationToken))
            {
                return NotFound("Unable to find Company");
            }
            
            if (await _dbContext.Companies
                    .AnyAsync(c => c.Title == companyModel.Title, cancellationToken))
            {
                return NotFound($"Company title named '{companyModel.Title}' already exists, please insert another name for title");
            }

            var dbCompany = await _dbContext.Companies
                    .SingleOrDefaultAsync(c => c.Id == id, cancellationToken);

            dbCompany.Title = companyModel.Title;
            dbCompany.Address = companyModel.Address;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return Ok();

        }
    }
}
