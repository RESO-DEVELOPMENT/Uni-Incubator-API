using Application.Domain.Models;

namespace Application.Persistence.Repositories
{
    public class NotificationRepository : BaseRepository<Notification, Guid>
    {
        public NotificationRepository(DataContext context) : base(context)
        {
        }
    }
}