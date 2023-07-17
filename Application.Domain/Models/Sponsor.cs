using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Application.Domain.Enums.Sponsor;

namespace Application.Domain.Models
{
  public class Sponsor
  {
    public Sponsor()
    {
      SponsorFiles = new List<SponsorFile>();
      ProjectSponsors = new List<ProjectSponsor>();
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid SponsorId { get; set; }

    public string SponsorName { get; set; } = null!;
    public string SponsorDescription { get; set; } = null!;
    public SponsorType Type { get; set; } = SponsorType.Bussiness;
    public string? ImageUrl { get; set; } = null!;

    public SponsorStatus SponsorStatus { get; set; } = SponsorStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTimeHelper.Now();

    public virtual List<SponsorFile> SponsorFiles { get; set; }
    public virtual List<ProjectSponsor> ProjectSponsors { get; set; }
  }
}