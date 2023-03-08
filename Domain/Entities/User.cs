using DataAnnotationsExtensions;
using Domain.Enums;
using Domain.Exceptions;
using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Domain.Entities
{
    public class User: BaseEntity
    {
        public string Name { get; set; }
        [Email]
        public string Email { get; set; }
        public double Score { get; set; }
        public Gender Gender { get; set; }
        public Role Role { get; set; }
        public string ProfilePhoto { get; set; }
        public Guid RefreshToken { get; set; }
        public DateTime ExpiryTime { get; set; }

        //Field
        [StringLength(20, MinimumLength = 10)]
        private string _password;

        [JsonIgnore]
        public string Password
        {
            get => _password;

            set
            {
                if (string.IsNullOrEmpty(value)) throw new PasswordNullException("New password cannot be null.");
                var passwordHasher = new PasswordHasher<User>();
                _password = passwordHasher.HashPassword(this, value);

            }

        }

        //Method
        public bool VerifyPassword(string password)
        {
            var passwordHasher = new PasswordHasher<User>();
            return passwordHasher
                .VerifyHashedPassword(this, Password, password) != PasswordVerificationResult.Failed;

        }
    }
}
