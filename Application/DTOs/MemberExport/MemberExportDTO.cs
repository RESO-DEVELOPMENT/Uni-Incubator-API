using Application.Domain.Enums.Project;

namespace Application.DTOs.MemberExport
{
    public class MemberExportDTO_ProjectTask
    {
        public string TaskName { get; set; } = null!;
        public string TaskDescription { get; set; } = null!;
    }

    public class MemberExportDTO_Project
    {
        public string ProjectName { get; set; } = null!;
        public string ProjectDescription { get; set; } = null!;
        public string ProjectRole { get; set; } = null!;
        public string ProjectShortName { get; set; } = null!;

        public ProjectStatus Status { get; set; }

        public int TotalWorkHours { get; set; }
        public int TotalTaskDone { get; set; }

        public List<MemberExportDTO_ProjectTask> Tasks { get; set; } = new();

    }

    public class MemberExportDTO
    {
        public string MemberName { get; set; } = null!;
        public string MemberEmail { get; set; } = null!;

        public List<MemberExportDTO_Project> Projects { get; set; } = new();
    }
}
