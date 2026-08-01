using Application.ErrorNamespace;

namespace Application.Models.Result
{
    public class Result<T> : Result
    {
        public T? Data { get; }

        private Result(bool isSuccess, T? data, Error? error = null) : base(isSuccess, error)
        {
            Data = data;
        }

        public static Result<T> Success(T data) => new Result<T>(true, data);

        public new static Result<T> Failure(Error err) => new Result<T>(false, default, err);
    }
}
