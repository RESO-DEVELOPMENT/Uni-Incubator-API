using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Application.Domain.Enums.Notification;

namespace Application.Domain.Models
{
  public class Notification
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid NotificationId { get; set; }

    public NotificationType Type { get; set; }
    public string Target { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;

    public bool IsRead { get; set; } = false;

    public Guid MemberId { get; set; }
    public virtual Member Member { get; set; } = null!;

    // public SponsorStatus SponsorStatus { get; set; } = SponsorStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTimeHelper.Now();
  }
}