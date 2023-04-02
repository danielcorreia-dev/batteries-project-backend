﻿using DataAnnotationsExtensions;
using Domain.Enums;
using Domain.Exceptions;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Domain.Entities
{
    public sealed class User: BaseEntity
    {
        public User()
        {
            List<User> users = new List<User>();
        }

        public List<UserCompanyPlace> Companies { get; set; }
        public string Nick { get; set; }
        [Email]
        public string Email { get; set; }
        public Guid RefreshToken { get; set; }
        public DateTime ExpiryTime { get; set; }
        public bool RememberMe { get; set; }
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
