using Application.Domain.Enums.ProjectSalaryCycle;

namespace Application.DTOs.ProjectSalaryCycle
{
    public class ProjectSalaryCycleDTO
    {
        public Guid ProjectSalaryCycleId { get; set; }
        // public ProjectDTO Project { get; set; } = null!;
        public ProjectSalaryCycleStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public double ProjectBudget { get; set; }
        public double TotalSponsorBudget { get; set; }
    }
}