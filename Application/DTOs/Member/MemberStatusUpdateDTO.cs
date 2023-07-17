using System.ComponentModel.DataAnnotations;
using Application.Domain.Enums.Member;

namespace Application.DTOs.Member;

public class MemberStatusUpdateDTO
{
    [Required]
    public Guid MemberId { get; set; }
    [Required]
    public MemberStatus Status { get; set; }

    // public IFormFile? ImageAsFile { get; set; }
}