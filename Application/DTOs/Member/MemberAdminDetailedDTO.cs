using Application.DTOs.MemberLevel;

namespace Application.DTOs.Member
{
    public class MemberAdminDetailedDTO
    {
        public Guid MemberId { get; set; }
        public string EmailAddress { get; set; } = null!;

        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? ImageUrl { get; set; } = null!;
        public string? FacebookUrl { get; set; } = null!;

        public MemberLevelDTO MemberLevels { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}