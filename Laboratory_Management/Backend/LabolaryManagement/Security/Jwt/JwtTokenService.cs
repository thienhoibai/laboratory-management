using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Security.Jwt
{
    public interface IJwtTokenService
    {
        (string accessToken, DateTime expiresAt, string refreshToken) IssueTokens(Guid userId, string username, IEnumerable<string> roles, IEnumerable<string>? permissions = null);
        ClaimsPrincipal? ValidateToken(string token, bool validateLifetime = true);
        string HashRefreshToken(string refreshToken);
    }

    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtOptions _options;
        private readonly JwtSecurityTokenHandler _handler = new JwtSecurityTokenHandler();
        private readonly SymmetricSecurityKey _key;

        public JwtTokenService(IOptions<JwtOptions> options)
        {
            _options = options.Value;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        }

        public (string accessToken, DateTime expiresAt, string refreshToken) IssueTokens(Guid userId, string username, IEnumerable<string> roles, IEnumerable<string>? permissions = null)
        {
            var now = DateTime.UtcNow;
            var expires = now.AddMinutes(_options.AccessTokenMinutes);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
            if (permissions != null)
            {
                foreach (var p in permissions)
                {
                    claims.Add(new Claim("perm", p));
                }
            }

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: now,
                expires: expires,
                signingCredentials: creds);

            var accessToken = _handler.WriteToken(token);
            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            return (accessToken, expires, refreshToken);
        }

        public ClaimsPrincipal? ValidateToken(string token, bool validateLifetime = true)
        {
            var parameters = new TokenValidationParameters
            {
                ValidIssuer = _options.Issuer,
                ValidAudience = _options.Audience,
                IssuerSigningKey = _key,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = validateLifetime,
                ClockSkew = TimeSpan.FromMinutes(1)
            };

            try
            {
                return _handler.ValidateToken(token, parameters, out _);
            }
            catch
            {
                return null;
            }
        }

        public string HashRefreshToken(string refreshToken)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(refreshToken));
            return Convert.ToHexString(bytes);
        }
    }
}
