using ITI_Project.Core.Errors;


namespace ITI_Project.Core.Common
{
    public class ServiceResult
    {
        public ServiceResult(bool isSuccess, Error error)
        {
            if((isSuccess && error != Error.None) || (!isSuccess && error == Error.None))
            {
                throw new ArgumentException("Success results cannot have an error.");
            }
            IsSuccess = isSuccess;
            Error = error;
        }

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public Error Error { get; }

        public static ServiceResult Success()
            => new(true, Error.None);

        public static ServiceResult Failure(Error error)
            => new(false, error);
    }

    public class ServiceResult<T> : ServiceResult
    {
        private readonly T? _data;
        public ServiceResult(bool isSuccess, Error error, T? data)
            : base(isSuccess, error)
        {
            _data = data;
        }
        public static ServiceResult<T> Success(T data)
            => new(true, Error.None, data);

        public new static ServiceResult<T> Failure(Error error)
            => new(false, error, default);
        public T Data => IsSuccess ? _data! : throw new InvalidOperationException("Failure results cannot have data");
    }

    // primary constructor syntax
    //public class ServiceResult<T>(bool isSuccess, Error error, T data) : ServiceResult(isSuccess, error)
    //{
    //    private readonly T? _data;
    //    public T Data => IsSuccess ? _data! : throw new InvalidOperationException("Failure results cannot have data");
    //}
}
