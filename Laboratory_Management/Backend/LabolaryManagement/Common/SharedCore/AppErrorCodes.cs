namespace Lab.Shared.Core
{
    public static class AppErrorCodes
    {
        public const string InvalidRequest = "INVALID_REQUEST"; // 400
        public const string AuthInvalidCredentials = "AUTH_INVALID_CREDENTIALS"; // 401
        public const string AuthTokenExpired = "AUTH_TOKEN_EXPIRED"; // 401
        public const string PermissionDenied = "PERMISSION_DENIED"; // 403
        public const string ResourceNotFound = "RESOURCE_NOT_FOUND"; // 404
        public const string ResourceConflict = "RESOURCE_CONFLICT"; // 409
        public const string DuplicateEmail = "DUPLICATE_EMAIL"; // 409
        public const string DuplicateUsername = "DUPLICATE_USERNAME"; // 409
        public const string ValidationError = "VALIDATION_ERROR"; // 422
        public const string RateLimited = "RATE_LIMITED"; // 429
        public const string InternalError = "INTERNAL_ERROR"; // 500
        public const string ServiceUnavailable = "SERVICE_UNAVAILABLE"; // 503
    }
}
