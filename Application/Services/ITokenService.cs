namespace Application.Services
{
  public interface ITokenService
  {
    string CreateToken(Guid userId, string email, string role);
  }
}