using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure
{
    public class Seeder
    {
        public static void Initialize(BatteriesProjectDbContext context)
        {
            if (!context.Users.Any())
            {
                var users = new List<User>
                {
                    new()
                    {
                        Name = "Eugenio Lopes",
                        Email = "eugenio-lopes@batteries.com",
                        Gender = Gender.Male,
                        Role = Role.Admin,
                        Score = 112.42,
                        CreatedAt = DateTimeOffset.Now,
                        CreatedBy = "eugeniolopes@batteries.com",
                        UpdatedAt = DateTimeOffset.Now,
                        UpdatedBy = "eugeniolopes@batteries.com",
                        ProfilePhoto = "https://imgv3.fotor.com/images/blog-richtext-image/10-profile-picture-ideas-to-make-you-stand-out.jpg",
                        Password = "Passwd123456#",
                        ExpiryTime = DateTime.Now.AddDays(1),
                        RefreshToken = Guid.NewGuid()

                    },
                    new()
                    {
                        Name = "Daniel Correia",
                        Email = "Daniel-Correia@batteries.com",
                        Gender = Gender.Male,
                        Role = Role.Admin,
                        Score = 110.32,
                        CreatedAt = DateTime.Now,
                        CreatedBy = "Daniel-Correia@batteries.com",
                        UpdatedAt = DateTime.Now,
                        UpdatedBy = "Daniel-Correia@batteries.com",
                        ProfilePhoto = "https://imgv3.fotor.com/images/blog-richtext-image/10-profile-picture-ideas-to-make-you-stand-out.jpg",
                        Password = "Passwd123456#",
                        ExpiryTime = DateTime.Now.AddDays(1),
                        RefreshToken = Guid.NewGuid()
                    },
                };
                context.Users.AddRange(users);
                context.SaveChanges();
            }
        }
    }
}