
using System.Security.Claims;
using CivicSense.Facade.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CivicSense.Web.UI.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notifications;

        public NotificationController(
            INotificationService notifications)
        {
            _notifications = notifications;
        }

        private int CurrentUserId
        {
            get
            {
                string? value = User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

                if (!int.TryParse(value, out int userId)
                    || userId <= 0)
                {
                    throw new UnauthorizedAccessException(
                        "Invalid authentication session.");
                }

                return userId;
            }
        }

        // GET /Notification
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var notifications = await _notifications
                .GetMineAsync(CurrentUserId);

            return View(notifications);
        }

        // POST /Notification/MarkRead/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead(int id)
        {
            if (id <= 0)
                return NotFound();

            try
            {
                await _notifications.MarkReadAsync(
                    id, CurrentUserId);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            // A notification belonging to another user
            // causes UnauthorizedAccessException.
            // GlobalExceptionHandler returns HTTP 403.
            TempData["SuccessMessage"] =
                "Notification marked as read.";

            return RedirectToAction(nameof(Index));
        }
    }
}
