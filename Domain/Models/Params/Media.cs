using System;
using Microsoft.AspNetCore.Http;

namespace Domain.Models.Params
{
    public class Media
    {
        public string Name { get; set; }
        public string Path { get; set; }
        
        public Media()
        {
            
        }
        
        public Media(string name, string path)
        {
            Name = name;
            Path = path;
        }

        public static Media Create(string metadataName, string metadataPath)
        {
            return new Media(metadataName, metadataPath);
        }
    }
}