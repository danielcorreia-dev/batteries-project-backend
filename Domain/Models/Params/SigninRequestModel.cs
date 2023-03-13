using Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Models.Params
{
    public class SigninRequestModel
    {

        [EmailAddress]
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Name { get; set; }
        public bool RememberMe { get; set; }

    }
}
