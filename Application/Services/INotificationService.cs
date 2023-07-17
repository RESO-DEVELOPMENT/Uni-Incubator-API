using Application.Domain.Enums.Notification;

namespace Application.Services
{
  public interface INotificationService
  {
    Task<bool> ReadNotification(Guid notificationId, string requesterEmail);
    Task<bool> SendNotification(Guid memberId, NotificationType type, string targetId, string title, string content, bool sendNoti = false, bool saveNoti = true);
  }
}