﻿using System.ComponentModel.DataAnnotations;

namespace Domain.Models.Params
{
    public record SignupRequestModel
    (
        string Nick,
        [Required]
        [EmailAddress]
        string Email,
        [Required]
        string Password
    );
}
