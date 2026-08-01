using Application.ErrorNamespace;

namespace Application.Models.Result
{
    public class Result
    {
        public bool IsSuccess { get;}
        public bool IsFailure => !IsSuccess; 
        public Error? Error { get; }

        protected Result(bool isSuccess, Error? error = null) {
            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Success() => new Result(true);
        public static Result Failure(Error error) => new Result(false, error);

    }


}
