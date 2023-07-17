using Application.Domain.Enums.ProjectSponsor;
using Application.DTOs.Sponsor;

namespace Application.DTOs.ProjectSponsor
{
    public class ProjectSponsorDetailedDTO
    {
        public Guid ProjectSponsorId { get; set; }
        public SponsorDetailedDTO Sponsor { get; set; } = null!;

        public ProjectSponsorStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public double LastDepositAmount {get;set;}
        public DateTime? LastDepositDate {get;set;}
    }
}