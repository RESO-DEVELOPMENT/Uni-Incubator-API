namespace Application.DTOs.User
{
    public class UserDTO
    {
        public Guid UserId { get; set; }
        public string EmailAddress { get; set; } = null!;
        public string Role { get; set; } = null!;
    }
}