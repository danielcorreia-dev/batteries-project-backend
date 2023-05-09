using System;

namespace Domain.Models.Results
{
    public record SigninResponseModel
    (
        string Nick,
        string AccessToken,
        Guid RefreshToken
    );
}
