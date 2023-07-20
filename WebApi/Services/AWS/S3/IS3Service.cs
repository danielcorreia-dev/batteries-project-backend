using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Domain.Models.Params;
using Microsoft.AspNetCore.Http;

namespace WebApi.Services.AWS.S3
{
    public interface IS3Service
    {
        Media Upload( Media metadata, IFormFile file);
        void Remove(string documentKey);
        Task<Stream> Download(string documentKey);
        Task<string> GeneratePreSignedUrl(string documentKey);
    }
}