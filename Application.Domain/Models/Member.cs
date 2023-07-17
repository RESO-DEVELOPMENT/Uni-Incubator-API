using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Application.Domain.Enums.Member;

namespace Application.Domain.Models
{
    public class Member
    {
        public Member()
        {
            MemberWallets = new List<MemberWallet>();
            MemberLevels = new List<MemberLevel>();
            ProjectMembers = new List<ProjectMember>();
            MemberFiles = new List<MemberFile>();
            MemberVouchers = new List<MemberVoucher>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid MemberId { get; set; }

        [StringLength(450)]
        public string EmailAddress { get; set; } = null!;

        public string FullName { get; set; } = null!;
        public string? PhoneNumber { get; set; } = null!;
        public string? ImageUrl { get; set; } = null!;
        public string? FacebookUrl { get; set; } = null!;

        public MemberStatus MemberStatus { get; set; } = MemberStatus.Available;

        public virtual List<MemberWallet> MemberWallets { get; set; }
        public virtual List<ProjectMember> ProjectMembers { get; set; }
        public virtual List<MemberLevel> MemberLevels { get; set; }
        public virtual List<MemberFile> MemberFiles { get; set; }
        public virtual List<MemberVoucher> MemberVouchers { get; set; }

        public string? CertCode { get; set; } = null!;

        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTimeHelper.Now();
        public DateTime? UpdatedAt { get; set; }
    }
}