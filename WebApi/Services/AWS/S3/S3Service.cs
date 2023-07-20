using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Domain.Models.Params;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace WebApi.Services.AWS.S3
{
    public class S3Service : IS3Service
    {
        
        private readonly IAmazonS3 _s3Client;
        private readonly IConfiguration _configuration;

        public S3Service(IAmazonS3 s3Client, IConfiguration configuration)
        {
            _s3Client = s3Client;
            _configuration = configuration;
        }
        
        public Media Upload(Media metadata, IFormFile file)
        {
            using var memoryStream = new MemoryStream();
            file.CopyTo(memoryStream);

            var fileTransferUtility = new TransferUtility(_s3Client);
            fileTransferUtility.Upload( new TransferUtilityUploadRequest
            {
                InputStream = memoryStream,
                Key = metadata.Path,
                BucketName = _configuration.GetSection("AWS")["BucketName"],
                ContentType = file.ContentType,
            });

            return Media.Create(metadata.Name, metadata.Path);
        }

        public async void Remove(string documentKey)
        {
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _configuration.GetSection("AWS")["BucketName"],
                Key = documentKey
            };
            
            await _s3Client.DeleteObjectAsync(deleteRequest);
        }

        public async Task<Stream> Download(string documentKey)
        {
            var downloadRequest = new GetObjectRequest
            {
                BucketName = _configuration.GetSection("AWS")["BucketName"],
                Key = documentKey
            };
            
            var fileStream = await _s3Client.GetObjectAsync(downloadRequest);

            return fileStream.ResponseStream;
        }
        
        public Task<string> GeneratePreSignedUrl(string documentKey)
        {
            var urlString = string.Empty;
            try
            {
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = _configuration.GetSection("AWS")["BucketName"],
                    Key = documentKey,
                    Expires = DateTime.Now.AddHours(12)
                };
                urlString = _s3Client.GetPreSignedURL(request);
            }
            catch(AmazonS3Exception e)
            {
                Console.WriteLine($"Error: '{e.Message}'");
            }

            return Task.FromResult(urlString);
        }
    }
}