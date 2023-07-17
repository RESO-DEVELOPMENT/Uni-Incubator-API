using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Application.Domain.Enums.Project;

namespace Application.Domain.Models
{
    public class Project
    {

        public Project()
        {
            ProjectMember = new List<ProjectMember>();
            ProjectSponsors = new List<ProjectSponsor>();
            ProjectReports = new List<ProjectReport>();
            ProjectFiles = new List<ProjectFile>();
            ProjectMemberRequests = new List<ProjectMemberRequest>();
            ProjectWallets = new List<ProjectWallet>();
            PayslipItems = new List<PayslipItem>();
            ProjectEndRequests = new List<ProjectEndRequest>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ProjectId { get; set; }

        public string ProjectName { get; set; } = null!;
        public string ProjectShortName { get; set; } = null!;
        public string ProjectShortDescription { get; set; } = null!;
        public string ProjectLongDescription { get; set; } = null!;

        public double Budget { get; set; }
        public ProjectStatus ProjectStatus { get; set; } = ProjectStatus.Created;
        public ProjectType ProjectType { get; set; } = ProjectType.Application;
        public ProjectVisibility ProjectVisibility { get; set; } = ProjectVisibility.Public;

        public virtual List<ProjectMember> ProjectMember { get; set; }
        public virtual List<ProjectMemberRequest> ProjectMemberRequests { get; set; }
        public virtual List<ProjectEndRequest> ProjectEndRequests { get; set; }
        public virtual List<ProjectSponsor> ProjectSponsors { get; set; }
        public virtual List<ProjectFile> ProjectFiles { get; set; }
        public virtual List<ProjectWallet> ProjectWallets { get; set; }

        public virtual List<ProjectReport> ProjectReports { get; set; }

        public virtual List<PayslipItem> PayslipItems { get; set; }


        public DateTime CreatedAt { get; set; } = DateTimeHelper.Now();

        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}