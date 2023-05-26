namespace Domain.Models.Params
{
    public record UserChangePasswordModel
    (
        string Email,
        string NewPassword,
        string Password
    );
}