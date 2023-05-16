using System;

namespace Domain.Models.Results
{
    public record RefreshResponseModel
    (
        string Nick,
        string AccessToken,
        Guid RefreshToken
    );
}