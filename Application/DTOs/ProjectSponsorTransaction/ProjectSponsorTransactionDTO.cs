using Application.Domain.Enums.ProjectSponsorTransaction;
using Application.DTOs.ProjectSponsor;
using Application.DTOs.Sponsor;

namespace Application.DTOs.ProjectSponsorTransaction
{
  public class ProjectSponsorTransactionDTO
  {
    public Guid ProjectSponsonTracsactionId { get; set; }
    // public Guid SalaryCycleId { get; set; }

    public ProjectSponsorDTO ProjectSponsor { get; set; } = null!;

    public ProjectSponsorTransactionStatus Status { get; set; }
    public double Amount { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
  }
}