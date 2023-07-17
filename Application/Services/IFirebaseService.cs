using Application.Domain.Enums.Notification;
using FirebaseAdmin.Auth;

namespace Application.Services
{
  public interface IFirebaseService
  {
    Task<bool> SendMessage(string token, NotificationType type, string targetId, string title, string content);
    Task<FirebaseToken?> VerifyIdToken(string token);
  }
}