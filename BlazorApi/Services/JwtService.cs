using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using BlazorApi.Models;

public class JwtService
{
    private readonly int _expiryMinutes;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtService(IConfiguration configuration)
    {
        _secretKey = configuration["Jwt:SecretKey"]
                     ?? Environment.GetEnvironmentVariable("JwtSecretKey")
                     ?? "NOTSAFENOTSAFENOTSAFENOTSAFENOTSAFENOTSAFENOTSAFENOTSAFE";

        _issuer = configuration["Jwt:Issuer"]
                  ?? Environment.GetEnvironmentVariable("JwtIssuer")
                  ?? "H2-2025-API";

        _audience = configuration["Jwt:Audience"]
                    ?? Environment.GetEnvironmentVariable("JwtAudience")
                    ?? "H2-2025-Client";

        _expiryMinutes = int.Parse(configuration["Jwt:ExpiryMinutes"]
                                   ?? Environment.GetEnvironmentVariable("JwtExpiryMinutes")
                                   ?? "60");

    }

    public string GenerateToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secretKey);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
        };

        // Add role claim if the user has a role
        if (user.Roles != null)
        {
            claims.Add(new Claim(ClaimTypes.Role, user.Roles.Name));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_expiryMinutes),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token,
                validationParameters, out SecurityToken validatedToken);
            return principal;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error validating token: " + ex.Message);
            return null;
        }
    }

    public string? GetUserIdFromToken(string token)
    {
        var principal = ValidateToken(token);
        return principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public Dictionary<string, string>? GetClaimsFromToken(string token)
    {
        var principal = ValidateToken(token);
        if (principal == null) return null;

        return principal.Claims.ToDictionary(c => c.Type, c => c.Value);
    }

    public bool IsTokenExpired(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            return jwtToken.ValidTo < DateTime.UtcNow;
        }
        catch
        {
            return true;
        }
    }
}