using Application.Domain.Enums.ProjectSalaryCycle;

namespace Application.DTOs.ProjectSalaryCycle
{
    public class ActiveProjectSalaryCycleDTO
    {
        public Guid ProjectSalaryCycleId { get; set; }
        public ProjectSalaryCycleStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public double EstimatedBudget { get; set; }
    }
}