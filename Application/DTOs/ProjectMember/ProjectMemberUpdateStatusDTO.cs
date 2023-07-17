using Application.Domain.Enums.ProjectMember;
using Application.DTOs.Member;

namespace Application.DTOs.ProjectMember
{
  public class ProjectMemberUpdateStatusDTO
  {
    public Guid ProjectMemberId { get; set; }
    public ProjectMemberStatus Status { get; set; }
  }
}