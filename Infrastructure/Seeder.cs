using Domain.Entities;
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
                        Nick = "Eugenio Lopes",
                        Email = "eugenio-lopes@batteries.com",
                        CreatedAt = DateTimeOffset.Now,
                        Password = "Passwd123456#",
                        ExpiryTime = DateTime.Now.AddDays(1),
                        RefreshToken = Guid.NewGuid()

                    },
                    new()
                    {
                        Nick = "Daniel Correia",
                        Email = "Daniel-Correia@batteries.com",
                        CreatedAt = DateTime.Now,
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