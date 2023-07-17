using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Application.Domain.Enums.ProjectSponsorTransaction;

namespace Application.Domain.Models
{
    // [PrimaryKey(nameof(ProjectId), nameof(MemberId))]
    public class ProjectSponsorTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ProjectSponsonTracsactionId { get; set; }

        public Guid ProjectSponsorId { get; set; }
        public virtual ProjectSponsor ProjectSponsor { get; set; } = null!;

        // public Guid? SalaryCycleId { get; set; }
        // public virtual SalaryCycle SalaryCycle { get; set; } = null!;

        public ProjectSponsorTransactionType Type { get; set; } = ProjectSponsorTransactionType.Deposit;
        public ProjectSponsorTransactionStatus Status { get; set; } = ProjectSponsorTransactionStatus.Paid;
        public double Amount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTimeHelper.Now();
        public DateTime? PaidAt { get; set; }
    }
}