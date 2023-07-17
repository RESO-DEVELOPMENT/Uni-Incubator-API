using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Member;

public class MemberUpdateDTO
{
    public string? FullName { get; set; }

    [Phone]
    [RegularExpression(@"([\d]{9,10})")]
    public string? PhoneNumber { get; set; }
    public string? FacebookUrl { get; set; }

    public string? ImageAsBase64 { get; set; }

    // public IFormFile? ImageAsFile { get; set; }
}