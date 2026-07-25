using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITI_Project.Core.Errors;

namespace ITI_Project.Core.Shared
{
    public class Result<T> : Result
    {
        private readonly T? value;

        private Result(
            T? value,
            bool isSuccess,
            Error error)
            : base(isSuccess, error)
        {
            this.value = value;
        }

        public T Value =>
            IsSuccess
            ? value!
            : throw new InvalidOperationException();

        public static Result<T> Success(T value)
            => new(value, true, Error.None);

        public static new Result<T> Failure(Error error)
            => new(default, false, error);
    }
}
