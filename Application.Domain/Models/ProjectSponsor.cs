using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Application.Domain.Enums.ProjectSponsor;

namespace Application.Domain.Models
{
    // [PrimaryKey(nameof(ProjectId), nameof(MemberId))]
    public class ProjectSponsor
    {
        public ProjectSponsor()
        {
            ProjectSponsorTransactions = new List<ProjectSponsorTransaction>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ProjectSponsorId { get; set; }

        public Guid ProjectId { get; set; }
        public virtual Project Project { get; set; } = null!;

        public Guid SponsorId { get; set; }
        public virtual Sponsor Sponsor { get; set; } = null!;

        public ProjectSponsorStatus Status { get; set; } = ProjectSponsorStatus.Available;
        public virtual List<ProjectSponsorTransaction> ProjectSponsorTransactions { get; set; }

        public DateTime CreatedAt { get; set; } = DateTimeHelper.Now();
        public DateTime? UpdatedAt { get; set; }
    }
}