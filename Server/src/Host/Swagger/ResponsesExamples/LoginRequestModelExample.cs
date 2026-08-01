using Application.UseCases.AuthUseCases.Login.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Swagger.ResponsesExamples
{
    public class LoginRequestModelExample : IExamplesProvider<LoginRequestModel>
    {
        public LoginRequestModel GetExamples()
        {
            return new LoginRequestModel(
                "user123@gmail.com",
                "sqfKsiAnyfNpD%8VU");
        }
    }
}
