
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Models.Params;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using WebApi.Services.AWS.S3;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class UserController : ControllerBase
    {
        private readonly BatteriesProjectDbContext _dbContext;
        private readonly IS3Service _s3Service;

        public UserController(
            BatteriesProjectDbContext dbContext,
            IS3Service s3Service)
        {
            _dbContext = dbContext;
            _s3Service = s3Service;
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
        [HttpGet("{id}/profile")]
        public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            
            var dbUser = await _dbContext.Users
                .AsNoTracking()
                .Where(u => u.Id == id)
                .Select(u => new
                {
                    Email = u.Email,
                    Nick = u.Nick,
                    ProfilePhoto = _s3Service.GeneratePreSignedUrl(u.ProfilePhoto).Result,
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
                .Select(u => new
                {
                  Id = u.Id,
                  Email = u.Email,
                  Nick = u.Nick,
                  ProfilePhoto = _s3Service.GeneratePreSignedUrl(u.ProfilePhoto).Result,
                  TotalScore = u.Companies.Sum(uc => uc.Score)
                })
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
                     Id = c.Id
                 })
                 .SingleOrDefaultAsync(cancellationToken);
            
            if(userCompany == null)
            {
                return NotFound("User has no company");
            }
            
            return Ok(userCompany);

        }
        
        /// <summary>
        /// Fazer upload da foto de perfil do usuário
        /// </summary>
        /// <param name="file">A foto de perfil do usuairo</param>
        /// <param name="id">O Id do usuario a que pertence a foto</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("{id}/profile-photo/upload")]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file, int id, CancellationToken cancellationToken)
        {
            
            if (!await _dbContext.Users.AnyAsync(u => u.Id == id, cancellationToken))
            {
                return NotFound("Unable to find User");
            }

            var dbUser = await _dbContext.Users
                    .SingleOrDefaultAsync(u => u.Id == id, cancellationToken);
            
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            var allowedExtensions = new List<string>() { ".jpg", ".jpeg", ".png" };
            if (!allowedExtensions.Contains(fileExtension))
                return BadRequest("File Format not supported");

            const int maxFileSize = 5000000;
            if (file.Length > maxFileSize)
                return BadRequest("File size too large");
            
            if(string.IsNullOrEmpty(dbUser.ProfilePhoto))
            {
                var newUniqueFileName = Guid.NewGuid().ToString();
            
                var media = new Media()
                {
                    Name = file.FileName,
                    Path = $"uploads/{newUniqueFileName}/{file.FileName}"
                };

                dbUser.ProfilePhoto = media.Path;
                await _dbContext.SaveChangesAsync(cancellationToken);

                var uploadedMedia = _s3Service.Upload(media, file);
            
                return Ok(uploadedMedia);
            }

            var uniqueFileName = Guid.NewGuid().ToString();
            
            var newMedia = new Media()
            {
                Name = file.FileName,
                Path = $"uploads/{uniqueFileName}/{file.FileName}"
            };

            _s3Service.Remove(dbUser.ProfilePhoto);
            
            dbUser.ProfilePhoto = newMedia.Path;
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            var newUploadedMedia = _s3Service.Upload(newMedia, file);
            
            return Ok(newUploadedMedia);
        }

        /// <summary>
        /// Baixar a foto de perfil do usuário
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{id}/profile-photo")]
        public async Task<IActionResult> DownloadProfilePicture(int id, CancellationToken cancellationToken)
        {
            var profilePhoto = await _dbContext.Users
                .Where(u => u.Id == id)
                .Select(u => u.ProfilePhoto)
                .FirstOrDefaultAsync(cancellationToken);

            if (string.IsNullOrEmpty(profilePhoto))
                NotFound("User has no profile photo");
            
            var contents = new Dictionary<string, Stream>
            {
                ["file"] = await _s3Service.Download(profilePhoto)
            };
            
            var fileNameWithExtension = Path.GetFileName(profilePhoto);
            var fileExtension = Path.GetExtension(fileNameWithExtension);
            var contentType = new FileExtensionContentTypeProvider().Mappings[fileExtension] ;

            return File(contents["file"], contentType, fileNameWithExtension);
        }
        
        /// <summary>
        /// Remover a foto de perfil do usuário
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("{id}/profile-photo/remove")]
        public async Task<IActionResult> RemoveProfilePhoto(int id, CancellationToken cancellationToken)
        {

            if (!await _dbContext.Users.AnyAsync(u => u.Id == id, cancellationToken))
            {
                return NotFound("Unable to find User");
            }

            var dbUser = await _dbContext.Users
                .SingleOrDefaultAsync(u => u.Id == id, cancellationToken);

            if (string.IsNullOrEmpty(dbUser.ProfilePhoto))
            {
                return NotFound("User has no profile photo");
            }

            _s3Service.Remove(dbUser.ProfilePhoto);

            dbUser.ProfilePhoto = null;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok("Profile photo removed");
        }
    }
}