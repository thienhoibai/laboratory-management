using System.Text.RegularExpressions;

namespace IAM.Application.Security
{
    public class PasswordPolicyOptions
    {
        public int MinLength { get; set; } = 8;
        public bool RequireUpper { get; set; } = true;
        public bool RequireLower { get; set; } = true;
        public bool RequireDigit { get; set; } = true;
        public bool RequireSymbol { get; set; } = true;
        public int HistoryNotAllowed { get; set; } = 5;
    }

    public interface IPasswordPolicy
    {
        PasswordPolicyOptions Options { get; }
        bool Validate(string password, out string? message);
    }

    public class PasswordPolicy : IPasswordPolicy
    {
        public PasswordPolicyOptions Options { get; } = new();

        public bool Validate(string password, out string? message)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < Options.MinLength)
            {
                message = $"Password must be at least {Options.MinLength} characters.";
                return false;
            }
            if (Options.RequireUpper && !Regex.IsMatch(password, "[A-Z]"))
            {
                message = "Password must contain an uppercase letter.";
                return false;
            }
            if (Options.RequireLower && !Regex.IsMatch(password, "[a-z]"))
            {
                message = "Password must contain a lowercase letter.";
                return false;
            }
            if (Options.RequireDigit && !Regex.IsMatch(password, "[0-9]"))
            {
                message = "Password must contain a digit.";
                return false;
            }
            if (Options.RequireSymbol && !Regex.IsMatch(password, @"[^A-Za-z0-9]"))
            {
                message = "Password must contain a symbol.";
                return false;
            }
            message = null;
            return true;
        }
    }
}
