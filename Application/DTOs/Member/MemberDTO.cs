namespace Application.DTOs.Member
{
  public class MemberDTO
  {
    public Guid MemberId { get; set; }
    public string EmailAddress { get; set; } = null!;

    public string FullName { get; set; } = null!;
    public string? ImageUrl { get; set; } = null!;
    public string? FacebookUrl { get; set; } = null!;
    public String PhoneNumber { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
  }
}