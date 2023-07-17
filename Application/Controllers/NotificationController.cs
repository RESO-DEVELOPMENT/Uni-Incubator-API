using Application.DTOs;
using Application.Helpers;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Application.Controllers
{
    public class NotificationController : BaseApiController
    {
        private INotificationService _notificationService { get; set; }

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [Authorize]
        [HttpPost("{notificationId}/read")]
        [SwaggerOperation("Set notification to already read")]
        public async Task<ResponseDTO<bool>> ReadNotification([FromRoute] Guid notificationId)
        {
            var result = await _notificationService.ReadNotification(notificationId, User.GetEmail());

            return result.FormatAsResponseDTO(200);
        }
    }
}