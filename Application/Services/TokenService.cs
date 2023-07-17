using Application.Helpers;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Domain;
using Application.Persistence.Repositories;

namespace Application.Services
{
  public class TokenService : ITokenService
  {
    private readonly SymmetricSecurityKey _key;

    public TokenService(IConfiguration config,
       UnitOfWork unitOfWork
    )
    {
      var secret = config.GetValue<string>("Authentication:JWTSecretKey");
      _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
    }

    public string CreateToken(Guid userId, string email, string role)
    {
      var claims = new Claim[]
          {
                    new Claim("id",userId.ToString()),
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.Role, role)
          };

      var tokenHandler = new JwtSecurityTokenHandler();
      var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

      var tokenDescriptor = new SecurityTokenDescriptor
      {
        Subject = new ClaimsIdentity(claims),
        Expires = DateTimeHelper.Now().AddDays(30),
        SigningCredentials = creds,
      };

      var token = tokenHandler.CreateToken(tokenDescriptor);
      return tokenHandler.WriteToken(token);
    }
  }
}