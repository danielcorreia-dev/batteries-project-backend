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
                        RememberMe = true,
                        CreatedAt = DateTimeOffset.Now,
                        Password = "Passwd123456#",
                        ExpiryTime = DateTime.Now.AddDays(30),
                        RefreshToken = Guid.NewGuid()

                    },
                    new()
                    {
                        Nick = "Daniel Correia",
                        Email = "daniel-correia@batteries.com",
                        RememberMe = true,
                        CreatedAt = DateTime.Now,
                        Password = "Passwd123456#",
                        ExpiryTime = DateTime.Now.AddDays(30),
                        RefreshToken = Guid.NewGuid()
                    },
                    new()
                    {
                        Nick = "Carlos Gama",
                        Email = "carlos-gama@batteries.com",
                        RememberMe = true,
                        CreatedAt = DateTimeOffset.Now,
                        Password = "Passwd123456#",
                        ExpiryTime = DateTime.Now.AddDays(30),
                        RefreshToken = Guid.NewGuid()

                    },
                };
                context.Users.AddRange(users);
                context.SaveChanges();
            };
            
            if (!context.Companies.Any())
            {
                var companies = new List<Company>
                {
                    new()
                    {
                        Title = "Company 01",
                        Address = "Rua Dom Felício Vasconcelos - Capiata",
                        CreatedAt = DateTimeOffset.Now,
                    },
                    new()
                    {
                        Title = "Company 02",
                        Address = "Ponta Verde",
                        CreatedAt = DateTimeOffset.Now,
                    },
                    new()
                    {
                        Title = "Company 03",
                        Address = "Rua Pedro Florentim Bastos - Polém",
                        CreatedAt = DateTimeOffset.Now,
                    },
                };
                context.Companies.AddRange(companies);
                context.SaveChanges();
            };

            if (!context.CompanyBenefits.Any())
            {
                var companyBenefits = new List<CompanyBenefit>
                {
                    new()
                    {
                        Benefit = "Plano de saúde, Vale alimentação, Vale bem-estar, Vale Ifood",
                        Description = "Descrição do benefício 01",
                        Disabled = false,
                        CompanyId = 1,
                        CreatedAt = DateTimeOffset.Now,
                        ScoreNeeded = 60,
                    },
                    new()
                    {
                        Benefit = "Plano de saúde, Vale alimentação",
                        Description = "Descrição do benefício 02",
                        Disabled = false,
                        CompanyId = 2,
                        CreatedAt = DateTimeOffset.Now,
                        ScoreNeeded = 70,
                    },
                    new()
                    {
                        Benefit = "Plano de saúde, Vale alimentação, Vale bem-estar",
                        Description = "Descrição do benefício 03",
                        Disabled = false,
                        CompanyId = 3,
                        CreatedAt = DateTimeOffset.Now,
                        ScoreNeeded = 50,
                    },
                };
                context.CompanyBenefits.AddRange(companyBenefits);
                context.SaveChanges();
            }
            
            if (!context.UserCompanyScores.Any())
            {
                var userCompanyScores = new List<UserCompanyScore>
                {
                    new()
                    {
                        Score = 100,
                        CompanyId = 1,
                        UserId = 1,
            
                    },
                    new()
                    {
                        Score = 80,
                        CompanyId = 2,
                        UserId = 2,
            
                    },
                    new()
                    {
                        Score = 60,
                        CompanyId = 3,
                        UserId = 2,
            
                    },
                };
                context.UserCompanyScores.AddRange(userCompanyScores);
                context.SaveChanges();
            }
        }
    }
}