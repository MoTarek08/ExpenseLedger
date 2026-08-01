namespace Application.UseCases.AuthUseCases.Register.Models
{
    public record RegisterRequestModel(string Email, string DisplayName, string Password, string PasswordConfirmation);

}
