namespace Application.DTOs.ProjectReport
{
    public class ProjectReportEstimateDTO_Member
    {
        public Guid MemberId { get; set; }
        public string MemberEmail { get; set; } = null!;

        // public double P1 { get; set; }
        public double P2 { get; set; }
        public double P3 { get; set; }
        public double TaskPoint { get; set; }
        public double XP { get; set; }

        public double Bonus { get; set; }
    }

    public class ProjectReportEstimateDTO
    {
        public double TotalP2{ get; set; } = 0;
        public double TotalP3{ get; set; } = 0;
        public double TotalTaskPoint { get; set; } = 0;
        public double TotalBonusPoint { get; set; } = 0;

        public List<ProjectReportEstimateDTO_Member> MemberRewards { get; set; } = new List<ProjectReportEstimateDTO_Member>();
    }
}