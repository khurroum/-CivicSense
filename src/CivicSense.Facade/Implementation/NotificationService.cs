
using CivicSense.DataAccess.Interface;
using CivicSense.Entities;
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

        public async Task<List<Notification>> GetMineAsync(
            int actorId)
        {
            // Verify the current user is active.
            await FacadeValidation.GetActiveUserAsync(
                _users, actorId);

            // Retrieve only notifications belonging to this user.
            return await _notifications.GetByUserAsync(actorId);
        }

        public async Task MarkReadAsync(
            int id, int actorId)
        {
            // Verify the current user is active.
            await FacadeValidation.GetActiveUserAsync(
                _users, actorId);

            var notification = await _notifications
                .GetByIdAsync(id);

            if (notification == null)
            {
                throw new KeyNotFoundException(
                    "Notification not found.");
            }

            // Prevent users from modifying other users' notifications.
            if (notification.UserId != actorId)
            {
                throw new UnauthorizedAccessException(
                    "You cannot access this notification.");
            }

            if (notification.IsRead)
            {
                return;
            }

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;

            await _notifications.UpdateAsync(notification);
        }
    }
}
