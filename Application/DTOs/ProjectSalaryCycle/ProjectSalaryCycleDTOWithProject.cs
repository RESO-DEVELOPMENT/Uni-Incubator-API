using Application.Domain.Enums.ProjectSalaryCycle;
using Application.DTOs.Project;

namespace Application.DTOs.ProjectSalaryCycle
{
    public class ProjectSalaryCycleWithProjectDTO
    {
        public Guid ProjectSalaryCycleId { get; set; }
        public ProjectWithFilesDTO Project { get; set; } = null!;
        public ProjectSalaryCycleStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public double ProjectBudget { get; set; }
        public double TotalSponsorBudget { get; set; }
    }
}