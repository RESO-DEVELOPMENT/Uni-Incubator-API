using Application.Domain.Enums.SalaryCycle;

namespace Application.DTOs.SalaryCycle
{
  public class SalaryCycleDTO
  {
    public Guid SalaryCycleId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }

    public String Name { get; set; }

    public SalaryCycleStatus Status { get; set; }
  }
}