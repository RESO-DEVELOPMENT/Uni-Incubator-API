using Application.Domain.Enums.Sponsor;

namespace Application.DTOs.Sponsor
{
  public class SponsorDetailedDTO
  {
    public Guid SponsorId { get; set; }
    public string SponsorName { get; set; } = null!;
    public string SponsorDescription { get; set; } = null!;
    public string? ImageUrl { get; set; } = null!;
    public SponsorStatus SponsorStatus { get; set; }
    public SponsorType Type { get; set; }

    public double TotalPoint { get; set; }
    public double TotalProjects { get; set; }
  }
}