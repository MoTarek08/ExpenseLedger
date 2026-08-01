using Application.UseCases.AuthUseCases.Register.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Swagger.ResponsesExamples
{
    public class RegisterRequestModelExample : IExamplesProvider<RegisterRequestModel>
    {
        public RegisterRequestModel GetExamples()
        {
            return new RegisterRequestModel(
                "user123@gmail.com",
                "User",
                "sqfKsiAnyfNpD%8VU",
                "sqfKsiAnyfNpD%8VU");
        }
    }
}
