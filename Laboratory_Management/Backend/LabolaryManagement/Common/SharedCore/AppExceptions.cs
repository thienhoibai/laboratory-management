using System.Net;

namespace Lab.Shared.Core
{
    public abstract class AppException : Exception
    {
        public abstract int StatusCode { get; }
        public abstract string AppCode { get; }
        public virtual object?[]? FieldErrors => null;
        protected AppException(string message) : base(message) { }
    }

    public class ValidationAppException : AppException
    {
        public override int StatusCode => 422;
        public override string AppCode => AppErrorCodes.ValidationError;
        public object[]? Errors { get; }
        public override object?[]? FieldErrors => Errors;
        public ValidationAppException(string message, object[]? errors = null) : base(message)
        {
            Errors = errors;
        }
    }

    public class NotFoundAppException : AppException
    {
        public override int StatusCode => 404;
        public override string AppCode => AppErrorCodes.ResourceNotFound;
        public NotFoundAppException(string message) : base(message) { }
    }

    public class ConflictAppException : AppException
    {
        public override int StatusCode => 409;
        public override string AppCode => AppErrorCodes.ResourceConflict;
        public ConflictAppException(string message) : base(message) { }
    }

    public class UnauthorizedAppException : AppException
    {
        public override int StatusCode => 401;
        public override string AppCode => AppErrorCodes.AuthInvalidCredentials;
        public UnauthorizedAppException(string message) : base(message) { }
    }

    public class ForbiddenAppException : AppException
    {
        public override int StatusCode => 403;
        public override string AppCode => AppErrorCodes.PermissionDenied;
        public ForbiddenAppException(string message) : base(message) { }
    }

    public class RateLimitedAppException : AppException
    {
        public override int StatusCode => 429;
        public override string AppCode => AppErrorCodes.RateLimited;
        public RateLimitedAppException(string message) : base(message) { }
    }

    public class InternalAppException : AppException
    {
        public override int StatusCode => 500;
        public override string AppCode => AppErrorCodes.InternalError;
        public InternalAppException(string message) : base(message) { }
    }
}
