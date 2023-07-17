using System.ComponentModel.DataAnnotations;
using Application.Domain.Enums.ProjectMemberRequest;

namespace Application.DTOs.ProjectMemberRequest
{
    public class ProjectMemberUpdateDTO
    {
    [Required]
    public Guid ProjectMemberId { get; set; }

    [Required]
    public bool? Graduated { get; set; }
    [Required]
    [Range(0, 10)]
    public double? YearOfExp { get; set; }
    [Required]
    public bool? HaveEnghlishCert { get; set; }

    [Required]
    [Range(1, 10)]
    public double? LeadershipSkill { get; set; }
    [Required]
    [Range(1, 10)]
    public double? CreativitySkill { get; set; }
    [Required]
    [Range(1, 10)]
    public double? ProblemSolvingSkill { get; set; }

    [Required]
    [Range(1, 10)]
    public double? PositiveAttitude { get; set; }
    [Required]
    [Range(1, 10)]
    public double? TeamworkSkill { get; set; }
    [Required]
    [Range(1, 10)]
    public double? CommnicationSkill { get; set; }
  }
}