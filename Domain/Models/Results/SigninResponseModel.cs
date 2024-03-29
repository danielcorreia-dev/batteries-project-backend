﻿using System;

namespace Domain.Models.Results
{
    public record SigninResponseModel
    (
        int Id,
        string AccessToken,
        Guid RefreshToken
    );
}
