namespace Application.DTOs.ProjetMilestone
{
    public class ProjectMilestoneDTO
    {
        public Guid ProjectMilestoneId { get; set; }

        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
    }
}