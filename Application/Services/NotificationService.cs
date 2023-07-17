using Application.Domain.Enums.Notification;
using Application.Domain.Models;
using Application.Helpers;
using Application.Persistence.Repositories;
using Application.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IFirebaseService _firebaseService;
        private readonly UnitOfWork _unitOfWork;
        private readonly IHubContext<UserHub> _userHubContext;
        private readonly MemberConnections _userConnections;

        public NotificationService(
          IFirebaseService firebaseService,
          UnitOfWork unitOfWork,
          IHubContext<UserHub> userHubContext,
          MemberConnections userConnections)
        {
            _firebaseService = firebaseService;
            _unitOfWork = unitOfWork;
            _userHubContext = userHubContext;
            _userConnections = userConnections;
        }

        public async Task<bool> SendNotification(Guid memberId, NotificationType type, string targetId, string title, string content, bool sendFirebaseNoti = false, bool saveNotification = true)
        {
            try
            {
                var hubUsers = _userConnections.GetUserConnections(memberId);
                await _userHubContext.Clients.Clients(hubUsers)
                  .SendAsync("NewNotification", new
                  {
                      type = type.ToString(),
                      targetId,
                      title,
                      content,
                  });

                var firebaseDeviceTokens = await _unitOfWork.UserFCMTokenRepository.GetQuery()
                  .Include(u => u.User)
                    .ThenInclude(u => u.Member)
                  .Where(t => t.User.Member.MemberId == memberId)
                  .ToListAsync();

                firebaseDeviceTokens.ForEach(async (token) =>
                {
                    await _firebaseService.SendMessage(token.Token, type, targetId, title, content);
                });

                if (saveNotification)
                {
                    _unitOfWork.NotificationRepository.Add(new Notification()
                    {
                        MemberId = memberId,
                        Type = type,
                        Target = targetId,
                        Title = title,
                        Content = content,
                        IsRead = false,
                    });

                    await _unitOfWork.SaveAsync();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ReadNotification(Guid notificationId, string requesterEmail)
        {
            var member = await _unitOfWork.MemberRepository.GetQuery()
            .Where(m => m.EmailAddress == requesterEmail)
            .FirstOrDefaultAsync();

            if (member == null) throw new NotFoundException();


            var notiDb = await _unitOfWork.NotificationRepository.GetQuery()
            .Where(n => notificationId == n.NotificationId)
            .FirstOrDefaultAsync();

            if (notiDb == null || notiDb.MemberId != member.MemberId)
                throw new BadRequestException("Notification not found!", ErrorNameValues.NotificationNotFound);

            notiDb.IsRead = true;
            _unitOfWork.NotificationRepository.Update(notiDb);

            return await _unitOfWork.SaveAsync();
        }
    }
}