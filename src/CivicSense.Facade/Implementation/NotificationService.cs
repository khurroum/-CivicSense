
using CivicSense.DataAccess.Interface;
using CivicSense.Entities;
using CivicSense.Facade.DTOs;
using CivicSense.Facade.Interface;

namespace CivicSense.Facade.Implementation
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notifications;
        private readonly IUserRepository _users;

        public NotificationService(
            INotificationRepository notifications,
            IUserRepository users)
        {
            _notifications = notifications;
            _users = users;
        }

        public async Task<List<NotificationDto>> GetMineAsync(
            int actorId)
        {
            await FacadeValidation.GetActiveUserAsync(
                _users, actorId);

            var notifications = await _notifications
                .GetByUserAsync(actorId);

            return notifications.Select(Map).ToList();
        }

        public async Task MarkReadAsync(
            int id, int actorId)
        {
            await FacadeValidation.GetActiveUserAsync(
                _users, actorId);

            var notification = await _notifications
                .GetByIdAsync(id);

            if (notification == null)
            {
                throw new KeyNotFoundException(
                    "Notification not found.");
            }

            if (notification.UserId != actorId)
            {
                throw new UnauthorizedAccessException(
                    "You cannot access this notification.");
            }

            if (notification.IsRead)
                return;

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;

            await _notifications.UpdateAsync(notification);
        }

        private static NotificationDto Map(
            Notification notification)
        {
            return new NotificationDto
            {
                Id = notification.Id,
                IssueId = notification.IssueId,
                Title = notification.Title,
                Message = notification.Message,
                NotificationType = notification.NotificationType,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                ReadAt = notification.ReadAt
            };
        }
    }
}
