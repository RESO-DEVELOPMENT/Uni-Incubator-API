namespace Application.DTOs.User
{
    public class UserLoginResultDTO
    {
        public UserDTO User { get; set; } = null!;
        public string Token { get; set; } = null!;
        public bool IsNewUser { get; set; } = false;
    }
}