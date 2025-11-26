using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Errors
{
    public static class ErrorCodes
    {
        public const string InvalidCredentials = "INVALID_CREDENTIALS";
        public const string InvalidRefreshToken = "INVALID_REFRESH_TOKEN";
        public const string AccountLocked = "ACCOUNT_LOCKED";
        public const string ValidationError = "VALIDATION_ERROR";
        public const string NotFound = "NOT_FOUND";
        public const string Forbidden = "FORBIDDEN";
        public const string Unauthorized = "UNAUTHORIZED";
        public const string Conflict = "CONFLICT";
        public const string CannotDeletePatient = "CANNOT_DELETE_PATIENT";
        public const string CannotDeleteDefaultRole = "CANNOT_DELETE_DEFAULT_ROLE";
        public const string RoleNameExists = "ROLE_NAME_EXISTS";
        public const string PasswordReused = "PASSWORD_REUSED";
        public const string InternalServerError = "INTERNAL_SERVER_ERROR";
        public const string DuplicateEmail = "DUPLICATE_EMAIL";
        public const string DuplicateUsername = "DUPLICATE_USERNAME";
        public const string RateLimited = "RATE_LIMITED";
        public const string ServiceUnavailable = "SERVICE_UNAVAILABLE";
        // Reset password
        public const string InvalidResetToken = "INVALID_RESET_TOKEN";
        public const string ResetTokenExpired = "RESET_TOKEN_EXPIRED";
        public const string ResetTokenUsed = "RESET_TOKEN_USED";
        // Google auth
        public const string InvalidGoogleToken = "INVALID_GOOGLE_TOKEN";
        public const string AccountLinkRequired = "ACCOUNT_LINK_REQUIRED";
        
        // Patient validation
        public const string DuplicateCitizenId = "DUPLICATE_CITIZEN_ID";
        public const string DuplicateInsuranceNumber = "DUPLICATE_INSURANCE_NUMBER";
        public const string InvalidFullName = "INVALID_FULL_NAME";
        public const string InvalidDateOfBirth = "INVALID_DATE_OF_BIRTH";
        public const string InvalidAge = "INVALID_AGE";
    }

    public record ErrorDefinition(string Code, int Status, string Title, string Type);

    public static class ErrorCatalog
    {
        private static readonly Dictionary<string, ErrorDefinition> _defs = new(StringComparer.OrdinalIgnoreCase)
        {
            // 400/422
            { ErrorCodes.ValidationError,     new("VALIDATION_ERROR", 422, "Validation failed", "https://api.example.com/errors/validation") },
            { ErrorCodes.InvalidResetToken,   new("INVALID_RESET_TOKEN", 400, "Invalid reset token", "https://api.example.com/errors/invalid-request") },
            { ErrorCodes.ResetTokenExpired,   new("RESET_TOKEN_EXPIRED", 400, "Reset token expired", "https://api.example.com/errors/invalid-request") },
            { ErrorCodes.ResetTokenUsed,      new("RESET_TOKEN_USED", 400, "Reset token used", "https://api.example.com/errors/invalid-request") },
            { ErrorCodes.InvalidFullName,     new("INVALID_FULL_NAME", 422, "Full name is required", "https://api.example.com/errors/validation") },
            { ErrorCodes.InvalidDateOfBirth,  new("INVALID_DATE_OF_BIRTH", 422, "Date of birth is required", "https://api.example.com/errors/validation") },
            { ErrorCodes.InvalidAge,          new("INVALID_AGE", 422, "Invalid age", "https://api.example.com/errors/validation") },

            // 401
            { ErrorCodes.InvalidCredentials,  new("AUTH_INVALID_CREDENTIALS", 401, "Unauthorized", "https://api.example.com/errors/auth") },
            { ErrorCodes.InvalidRefreshToken, new("AUTH_TOKEN_EXPIRED", 401, "Unauthorized", "https://api.example.com/errors/auth") },
            { ErrorCodes.Unauthorized,        new("AUTH_INVALID_CREDENTIALS", 401, "Unauthorized", "https://api.example.com/errors/auth") },
            { ErrorCodes.InvalidGoogleToken,  new("INVALID_GOOGLE_TOKEN", 401, "Invalid Google token", "https://api.example.com/errors/auth") },

            // 403
            { ErrorCodes.Forbidden,           new("PERMISSION_DENIED", 403, "Forbidden", "https://api.example.com/errors/forbidden") },

            // 404
            { ErrorCodes.NotFound,            new("RESOURCE_NOT_FOUND", 404, "Resource not found", "https://api.example.com/errors/not-found") },

            // 409
            { ErrorCodes.Conflict,            new("RESOURCE_CONFLICT", 409, "Conflict", "https://api.example.com/errors/conflict") },
            { ErrorCodes.DuplicateEmail,      new("DUPLICATE_EMAIL", 409, "Email already exists", "https://api.example.com/errors/conflict") },
            { ErrorCodes.DuplicateUsername,   new("DUPLICATE_USERNAME", 409, "Username already exists", "https://api.example.com/errors/conflict") },
            { ErrorCodes.DuplicateCitizenId,  new("DUPLICATE_CITIZEN_ID", 409, "Citizen ID already exists", "https://api.example.com/errors/conflict") },
            { ErrorCodes.DuplicateInsuranceNumber, new("DUPLICATE_INSURANCE_NUMBER", 409, "Insurance number already exists", "https://api.example.com/errors/conflict") },
            { ErrorCodes.RoleNameExists,      new("RESOURCE_CONFLICT", 409, "Role name already exists", "https://api.example.com/errors/conflict") },
            { ErrorCodes.AccountLinkRequired, new("ACCOUNT_LINK_REQUIRED", 409, "Account link required", "https://api.example.com/errors/conflict") },

            // 423 (internal lock mapping)
            { ErrorCodes.AccountLocked,       new("PERMISSION_DENIED", 423, "Account is locked", "https://api.example.com/errors/forbidden") },

            // 429
            { ErrorCodes.RateLimited,         new("RATE_LIMITED", 429, "Too many requests", "https://api.example.com/errors/rate-limited") },

            // 500/503
            { ErrorCodes.InternalServerError, new("INTERNAL_ERROR", 500, "An unexpected error occurred", "https://api.example.com/errors/internal") },
            { ErrorCodes.ServiceUnavailable,  new("SERVICE_UNAVAILABLE", 503, "Service unavailable", "https://api.example.com/errors/service-unavailable") },

            // Domain specific
            { ErrorCodes.CannotDeletePatient, new("RESOURCE_CONFLICT", 409, "Cannot delete patient", "https://api.example.com/errors/conflict") },
            { ErrorCodes.CannotDeleteDefaultRole, new("RESOURCE_CONFLICT", 409, "Cannot delete default role", "https://api.example.com/errors/conflict") },
            { ErrorCodes.PasswordReused,      new("VALIDATION_ERROR", 400, "Password was used recently", "https://api.example.com/errors/invalid-request") },
        };

        public static bool TryGet(string code, out ErrorDefinition def) => _defs.TryGetValue(code, out def!);

        public static ErrorDefinition Get(string code)
        {
            if (!_defs.TryGetValue(code, out var def))
            {
                def = _defs[ErrorCodes.InternalServerError];
            }
            return def;
        }
    }
}
