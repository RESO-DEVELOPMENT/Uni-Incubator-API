using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Project
{
    // public class ProjectCreateDTO_Members
    // {
    //   [Required]
    //   [EmailAddress]
    //   public String MemberEmail { get; set; } = null!;
    //   [Required]
    //   public String Major { get; set; } = null!;
    // }

    public class ProjectCreateDTO
    {
        [Required]
        public string ProjectName { get; set; } = null!;
        [Required]
        [MaxLength(7)]
        public string ProjectShortName { get; set; } = null!;
        // [Required]
        // [StringLength(200)]
        // public String ProjectShortDescription { get; set; } = null!;
        [Required]
        public string ProjectDescription { get; set; } = null!;
        [Required]
        public string ProjectManagerEmail { get; set; } = null!;
        [Range(1, 1000000)]
        [Required]
        public double Budget { get; set; }

        public bool SendEmailToPM { get; set; } = false;
        // [Required]
        // public List<ProjectCreateDTO_Members> ProjectMembers { get; set; } = new List<ProjectCreateDTO_Members>();
    }
}