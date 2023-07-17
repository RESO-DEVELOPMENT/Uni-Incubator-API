using Application.Domain.Enums.Notification;

namespace Application.DTOs.Notification
{
  public class NotificationDTO
  {
    public Guid NotificationId { get; set; }

    public NotificationType Type { get; set; }
    public string Target { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }
  }
}