namespace Application.DTOs.ProjectSponsor
{
    public class ListProjectSponsorDTO
    {
        public List<ProjectSponsorDTO> Sponsors { get; set; } = new List<ProjectSponsorDTO>();
        public double TotalSponsorBudget { get; set; }
    }
}