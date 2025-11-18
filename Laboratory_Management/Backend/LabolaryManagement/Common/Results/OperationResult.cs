using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Results
{
    public class OperationResult
    {
        public bool Succeeded { get; set; }
        public string? Error { get; set; }

        public static OperationResult Success() => new OperationResult { Succeeded = true };
        public static OperationResult Fail(string error) => new OperationResult { Succeeded = false, Error = error };
    }

    public class OperationResult<T> : OperationResult
    {
        public T? Data { get; set; }

        public static OperationResult<T> Success(T data) => new OperationResult<T> { Succeeded = true, Data = data };
        public new static OperationResult<T> Fail(string error) => new OperationResult<T> { Succeeded = false, Error = error };
    }
}
