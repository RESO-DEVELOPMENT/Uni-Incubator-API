using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Application.Domain.Enums.User;

namespace Application.Domain.Models
{
    public class User
    {
        public User()
        {
            UserFCMTokens = new List<UserFCMToken>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid UserId { get; set; }

        // Credentials
        [StringLength(450)]
        public string EmailAddress { get; set; } = null!;
        public string? Password { get; set; } = null!;

        public LoginType LoginType { get; set; }

        public string RoleId { get; set; } = null!;
        public virtual Role Role { get; set; } = null!;

        public UserStatus UserStatus { get; set; } = UserStatus.Available;
        public DateTime CreatedAt { get; set; } = DateTimeHelper.Now();

        public string? PinCode { get; set; }
        public int FailedPincodeAttempt { get; set; } = 0;
        public DateTime? FailedPincodeLastAttemptTime { get; set; }

        public string? PasswordChangeToken { get; set; }
        public DateTime? PasswordChangeTokenLastSent { get; set; }
        public DateTime? PasswordChangeTokenExpiredAt { get; set; }

        public virtual List<UserFCMToken> UserFCMTokens { get; set; }

        public virtual Member Member { get; set; } = null!;
    }
}