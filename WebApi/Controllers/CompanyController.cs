using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Models.Params;
using Domain.Models.Results;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Services.AWS.S3;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class CompanyController: ControllerBase
    {

        private readonly BatteriesProjectDbContext _dbContext;
        private readonly IS3Service _s3Service;
        public CompanyController(
            BatteriesProjectDbContext dbContext,
            IS3Service s3Service)
        {
            _dbContext = dbContext;
            _s3Service = s3Service;
        }

        /// <summary>
        /// Listar todas as empresas
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAllCompaniesAsync(CancellationToken cancellationToken)
        {
            if (!await _dbContext.Companies.AnyAsync(cancellationToken))
            {
                return NotFound("Unable to find companies");
            };
            
            var companies = await _dbContext.Companies
                .AsNoTracking()
                .Select(c => new
                {
                    Id = c.Id,
                    Title = c.Title,
                    Address = c.Address,
                    OpeningHours = c.OpeningHours,
                    PhoneNumber = c.PhoneNumber
                })
                .ToListAsync(cancellationToken);

            return Ok(companies);
        }


       /// <summary>
       /// Filtrar as empresas pelo título, caso seja passado nenhum filtro, retorna todas os companies
       /// </summary>
       /// <param name="cancellationToken">Usado para cancelar a requisição</param>
       /// <returns>As empresas filtradas pelo seu Title, caso nao seja passado nada como parametro de filtro, retorna todos os empresas</returns>
       //GET: company/by-title
       [HttpGet("by-title")]
        public async Task<IActionResult> GetCompaniesByTitleAsync(CancellationToken cancellationToken)
        {
            if(!await _dbContext.Companies.AnyAsync(cancellationToken))
            {
                return NotFound("Unable to find Companies");
            }
            
            HttpContext.Request
                .Query
                .TryGetValue("Title", out var title);
            
            if (string.IsNullOrEmpty(title))
            {
                return Ok(await _dbContext.Companies
                    .AsNoTracking()
                    .ToListAsync(cancellationToken));
            }
            
            var dbCompanies = await _dbContext.Companies
                .AsNoTracking()
                .Where(c => c.Title.ToLower().Contains(title.ToString().ToLower()))
                .Select(c => new
                {
                    Id = c.Id,
                    Title = c.Title,
                    Address = c.Address,
                    ProfilePhoto = _s3Service.GeneratePreSignedUrl(c.User.ProfilePhoto).Result,
                    OpeningHours = c.OpeningHours,
                    PhoneNumber = c.PhoneNumber
                })
                .ToListAsync(cancellationToken);

            return Ok(dbCompanies);
        }
        
            
            
        /// <summary>
        /// Listar os dados de perfil da empresa
        /// </summary>
        /// <param name="id">O id da empresa</param>
        /// <param name="cancellationToken">Usado para cancelar a requisição</param>
        /// <returns>Ok({id, titulo, endereco})</returns>
        [AllowAnonymous]
        [HttpGet("{id}/profile")]
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
                    CompanyId = c.Id,
                    ProfilePhoto = _s3Service.GeneratePreSignedUrl(c.User.ProfilePhoto).Result,
                    Title = c.Title,
                    Address = c.Address,
                    OpenHours = c.OpeningHours,
                    PhoneNumber = c.PhoneNumber,
                    Benefit = c.Benefits.Select(cb => new
                    {
                        Id = cb.Id,
                        benefit = cb.Benefit,
                        Description = cb.Description,
                        ScoreNeeded = cb.ScoreNeeded,
                        Status = cb.Disabled
                    })
                    
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

            if (!ValidatePhoneNumber(company.PhoneNumber))
            {
                return BadRequest("Invalid phone number");
            }
            
            if (await _dbContext.Companies
                    .AnyAsync(c => c.Title == company.Title || c.PhoneNumber == company.PhoneNumber, cancellationToken))
            {
                return BadRequest("Company already exists");
            }

            var newCompany = new Company()
            {
                Title = company.Title,
                Address = company.Address,
                CreatedAt = DateTimeOffset.Now,
                OpeningHours = company.OpeningHours,
                PhoneNumber = company.PhoneNumber,
                Benefits = new List<CompanyBenefit>(),
                Users = new List<UserCompanyScore>(),
                UserId = company.UserId
            }; 

            await _dbContext.Companies.AddAsync(newCompany, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Created(nameof(GetByIdAsync), newCompany);

        }
        
        [NonAction]
        private bool ValidatePhoneNumber(string phoneNumber)
        {
            
            /* The format of the phone number must be: 
           
                (XX) 9XXXX-XXXX
            */
            Regex regExpPhoneNumber = new(@"^(\([0-9]{2})\)[ ]{1}(9[0-9]{4})-([0-9]{4})$");
            
            var isValid = regExpPhoneNumber.IsMatch(phoneNumber);

            if (!isValid)
            {
                return false;
            }
            
            return isValid;
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
        /// Desabilitar todos os beneficios de uma empresa
        /// </summary>
        /// <param name="id">O id da empresa</param>
        /// <param name="cancellationToken">Usado para cancelar a requisição</param>
        /// <returns>Ok()</returns>
        //DELETE: company/{id}/benefits
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
        /// Atualizar beneficio da empresa
        /// </summary>
        /// <param name="companyId">O id da empresa</param>
        /// <param name="benefitId">O id do beneficio</param>
        /// <param name="companyBenefit">o objeto companyBenefits contendo os novos beneficios</param>
        /// <param name="cancellationToken">Usado para cancelar a requisição</param>
        /// <returns>Created()</returns>
        [HttpPut("{companyId}/benefits/{benefitId}/update")]
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
        /// <param name="userCompanyScoreModelParams">O novo UserCompanyScore a ser inserido no banco</param>
        /// <param name="cancellationToken">Usado para cancelar a requisição</param>
        /// <returns>Created()</returns>
      [HttpPost("{id}/user")]
      public async Task<IActionResult> PostUserCompanyScoresAsync(int id, [FromBody] UserCompanyScoreModelParams userCompanyScoreModelParams, CancellationToken cancellationToken)
      {
          if (id != userCompanyScoreModelParams.CompanyId)
          {
              return BadRequest("The CompanyId of the URL is different from the CompanyId of the body");
          }

          var companyExists = await _dbContext.Companies.AnyAsync(c => c.Id == id, cancellationToken);
          if (!companyExists)
          {
              return NotFound("Unable to find company");
          }

          var userExists = await _dbContext.Users.AnyAsync(u => u.Id == userCompanyScoreModelParams.UserId, cancellationToken);
          if (!userExists)
          {
              return NotFound("Unable to find user");
          }

          var existingUserCompanyScore = await _dbContext.UserCompanyScores.FirstOrDefaultAsync(ucs =>
              ucs.CompanyId == id && ucs.UserId == userCompanyScoreModelParams.UserId, cancellationToken);

          if (existingUserCompanyScore != null)
          {
              existingUserCompanyScore.Score += userCompanyScoreModelParams.Scores;
          }
          else
          {
              var newUserCompanyScore = new UserCompanyScore
              {
                  Score = userCompanyScoreModelParams.Scores,
                  CompanyId = userCompanyScoreModelParams.CompanyId,
                  UserId = userCompanyScoreModelParams.UserId
              };

              await _dbContext.UserCompanyScores.AddAsync(newUserCompanyScore, cancellationToken);
          }

          await _dbContext.SaveChangesAsync(cancellationToken);

          var result = new UserCompanyScoreModelResult
          {
              Scores = existingUserCompanyScore?.Score ?? userCompanyScoreModelParams.Scores,
              UserId = userCompanyScoreModelParams.UserId,
              CompanyId = userCompanyScoreModelParams.CompanyId
          };

          return Created(nameof(GetByIdAsync), result);
      }

        
        /// <summary>
        /// Adicionar um novo beneficio a uma empresa
        /// </summary>
        /// <param name="id">O id da empresa</param>
        /// <param name="companyBenefitsModel">O novo beneficio a ser adicionado </param>
        /// <param name="cancellationToken">Usado para cancelar a requisição</param>
        /// <returns>O endpoint de onde obter o novo beneficio criado e o beneficio criado</returns>
        //POST: company/{id}/benefits
        [HttpPost("{id}/benefits")]
        public async Task<IActionResult> PostCompanyBenefit(int id,CompanyBenefitsModel companyBenefitsModel, CancellationToken cancellationToken)
        {
            if (!await _dbContext.Companies
                    .AnyAsync(c => c.Id == id, cancellationToken))
            {
                return NotFound("Unable to find company");
            }

            var dbCompany = await _dbContext.Companies
                .Where(c => c.Id == id)
                .Include(c => c.Benefits)
                .SingleOrDefaultAsync(cancellationToken);

            var newCompanyBenefit = new CompanyBenefit()
            {
                Benefit = companyBenefitsModel.Benefit,
                Description = companyBenefitsModel.Description,
                ScoreNeeded = companyBenefitsModel.ScoreNeeded,
                Disabled = true,
                CreatedAt = DateTimeOffset.Now
            };

            dbCompany.Benefits.Add(newCompanyBenefit);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Created(nameof(GetByIdAsync), newCompanyBenefit);
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
            dbCompany.PhoneNumber = companyModel.PhoneNumber;
            dbCompany.OpeningHours = companyModel.OpeningHours;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return Ok();

        }
        /// <summary>
        /// Desabilitar ou habilitar o beneficio de uma empresa
        /// </summary>
        /// <param name="companyId">O id da empresa que contém o beneficio</param>
        /// <param name="benefitId">O id do beneficio a ser desabilitado ou habilitado</param>
        /// <param name="cancellationToken">Usado para cancelar a requisição</param>
        /// <returns></returns>
        //PUT: company/{id}/benefits/{id}?Active={active}
        [HttpPut("{companyId}/benefits/{benefitId}")]
        public async Task<IActionResult> PutCompanyBenefitByIdAsync(int companyId, int benefitId, CancellationToken cancellationToken)
        {

            var isExistsActive = Request.Query
                .TryGetValue("Active", out var active);

            if (!isExistsActive)
            {
                return BadRequest("Active query string is required");
            }
            
            if (!await _dbContext.Companies
                    .AnyAsync(c => c.Id == companyId,cancellationToken))
            {
                return NotFound("Unable to find company");
            }

            if (!await _dbContext.Companies
                        .Where(c => c.Id == companyId)
                        .SelectMany(c => c.Benefits)
                        .AnyAsync(cb => cb.Id == benefitId, cancellationToken))
            {
                return NotFound("Unable to find benefit");
            }

            var companyBenefit = await _dbContext.Companies
                .Where(c => c.Id == companyId)
                .Include(c => c.Benefits)
                .SelectMany(cb => cb.Benefits)
                .SingleOrDefaultAsync(cb => cb.Id == benefitId, cancellationToken);
                
                
            companyBenefit.Disabled = bool.TryParse(active, out var isActive) && isActive;
                
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok();
        }
    
        /// <summary>
        /// Desabilitar um benefício de uma empresa
        /// </summary>
        /// <param name="companyId">O id da empresa</param>
        /// <param name="benefitId">O id do benefício</param>
        /// <param name="cancellationToken">Usado para cancelar a requisição</param>
        /// <returns>NoContent()</returns>
        [HttpDelete("{companyId}/benefits/{benefitId}")]
        public async Task<IActionResult> DeleteBenefit(int companyId, int benefitId, CancellationToken cancellationToken)
        {
            var company = await _dbContext.Companies.FindAsync(companyId);
            if (company == null)
            {
                return NotFound("Unable to find company");
            }

            if (!await _dbContext.Companies
                        .Where(c => c.Id == companyId)
                        .SelectMany(c => c.Benefits)
                        .AnyAsync(cb => cb.Id == benefitId, cancellationToken))
            {
                return NotFound("Unable to find benefit");
            }

            var benefit = await _dbContext.Companies
                .Where(c => c.Id == companyId)
                .Include(c => c.Benefits)
                .SelectMany(cb => cb.Benefits)
                .SingleOrDefaultAsync(cb => cb.Id == benefitId, cancellationToken);
                
            _dbContext.CompanyBenefits.Remove(benefit);
            
            await _dbContext.SaveChangesAsync(cancellationToken);

            return NoContent();
        }
    }
}