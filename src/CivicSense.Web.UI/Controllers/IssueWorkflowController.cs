
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using CivicSense.Entities;
using CivicSense.Facade.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CivicSense.Web.UI.Controllers
{
    [Authorize]
    public class IssueWorkflowController : Controller
    {
        private readonly IIssueWorkflowService _workflow;

        public IssueWorkflowController(
            IIssueWorkflowService workflow)
        {
            _workflow = workflow;
        }

        // The user ID always comes from the
        // authenticated cookie, never from a form.
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

        // ========================================
        // ADMIN: REVIEW QUEUE
        // GET /IssueWorkflow/AdminQueue
        // ========================================

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminQueue()
        {
            var issues = await _workflow
                .GetAdminQueueAsync(CurrentUserId);

            return View(issues);
        }

        // ========================================
        // ADMIN: MOVE ISSUE TO UNDER REVIEW
        // POST /IssueWorkflow/Review/5
        // ========================================

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(int id)
        {
            return await RunCommandAsync(
                () => _workflow.ReviewAsync(
                    id, CurrentUserId),
                "Issue moved to Under Review.",
                nameof(AdminQueue));
        }

        // ========================================
        // ADMIN: ASSIGNMENT FORM
        // GET /IssueWorkflow/Assign/5
        // ========================================

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Assign(int id)
        {
            if (id <= 0)
                return NotFound();

            var queue = await _workflow
                .GetAdminQueueAsync(CurrentUserId);

            var issue = queue.FirstOrDefault(x =>
                x.IssueId == id &&
                x.Status == IssueStatus.UnderReview);

            if (issue == null)
            {
                TempData["ErrorMessage"] =
                    "Only issues under review can be assigned.";

                return RedirectToAction(
                    nameof(AdminQueue));
            }

            var staff = await _workflow
                .GetActiveStaffAsync(CurrentUserId);

            ViewBag.StaffOptions = new SelectList(
                staff,
                "Id",
                "FullName");

            return View(issue);
        }

        // ========================================
        // ADMIN: SAVE ASSIGNMENT
        // POST /IssueWorkflow/Assign/5
        // ========================================

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(
            int id, int staffId)
        {
            if (id <= 0)
                return NotFound();

            if (staffId <= 0)
            {
                TempData["ErrorMessage"] =
                    "Please select a Staff member.";

                return RedirectToAction(
                    nameof(Assign), new { id });
            }

            return await RunCommandAsync(
                () => _workflow.AssignAsync(
                    id, staffId, CurrentUserId),
                "Issue assigned successfully.",
                nameof(AdminQueue));
        }

        // ========================================
        // ADMIN: REJECT ISSUE
        // POST /IssueWorkflow/Reject/5
        // ========================================

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(
            int id, string reason)
        {
            return await RunCommandAsync(
                () => _workflow.RejectAsync(
                    id, CurrentUserId, reason),
                "Issue rejected successfully.",
                nameof(AdminQueue));
        }

        // ========================================
        // STAFF: MY ASSIGNED ISSUES
        // GET /IssueWorkflow/MyAssignments
        // ========================================

        [HttpGet]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> MyAssignments()
        {
            var assignments = await _workflow
                .GetMyAssignmentsAsync(CurrentUserId);

            return View(assignments);
        }

        // ========================================
        // STAFF: START WORK
        // POST /IssueWorkflow/StartWork/5
        // ========================================

        [HttpPost]
        [Authorize(Roles = "Staff")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartWork(int id)
        {
            return await RunCommandAsync(
                () => _workflow.StartWorkAsync(
                    id, CurrentUserId),
                "Work started successfully.",
                nameof(MyAssignments));
        }

        // ========================================
        // STAFF: RESOLVE ISSUE
        // POST /IssueWorkflow/Resolve/5
        // ========================================

        [HttpPost]
        [Authorize(Roles = "Staff")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Resolve(int id)
        {
            return await RunCommandAsync(
                () => _workflow.ResolveAsync(
                    id, CurrentUserId),
                "Issue marked as resolved.",
                nameof(MyAssignments));
        }

        // ========================================
        // ADMIN / ASSIGNED STAFF: STATUS HISTORY
        // GET /IssueWorkflow/History/5
        // ========================================

        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> History(int id)
        {
            if (id <= 0)
                return NotFound();

            var history = await _workflow
                .GetHistoryAsync(id, CurrentUserId);

            ViewBag.IssueId = id;

            return View(history);
        }

        // ========================================
        // SHARED MVC FORM ERROR HANDLING
        // ========================================

        private async Task<IActionResult> RunCommandAsync(
            Func<Task> operation,
            string successMessage,
            string redirectAction)
        {
            try
            {
                await operation();

                TempData["SuccessMessage"] =
                    successMessage;
            }
            catch (ValidationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["ErrorMessage"] =
                    "Another request changed this issue. " +
                    "Refresh the page and try again.";
            }
            catch (DbUpdateException ex)
                when (IsUniqueConflict(ex))
            {
                TempData["ErrorMessage"] =
                    "A conflicting database record already " +
                    "exists. Refresh and try again.";
            }
            catch (KeyNotFoundException)
            {
                TempData["ErrorMessage"] =
                    "The requested issue was not found.";
            }

            // Authorization failures and unexpected
            // database failures are handled by
            // GlobalExceptionHandler.
            return RedirectToAction(redirectAction);
        }

        private static bool IsUniqueConflict(
            DbUpdateException exception)
        {
            return exception.InnerException
                is SqlException sql &&
                (sql.Number == 2601 ||
                 sql.Number == 2627);
        }
    }
}
