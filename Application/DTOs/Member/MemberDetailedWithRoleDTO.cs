using Application.Domain.Enums.Member;
using Application.DTOs.MemberLevel;
using Application.DTOs.Role;

namespace Application.DTOs.Member
{
    public class MemberDetailedWithRoleDTO
    {
        public Guid MemberId { get; set; }
        public string EmailAddress { get; set; } = null!;

        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? ImageUrl { get; set; } = null!;
        public string? FacebookUrl { get; set; } = null!;
        public MemberStatus MemberStatus { get; set; }
        public MemberLevelDTO MemberLevels { get; set; } = null!;
        public RoleDTO Role { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}