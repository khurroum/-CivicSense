
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using CivicSense.Facade.DTOs;
using CivicSense.Facade.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CivicSense.Web.UI.Controllers
{
    [Authorize]
    public class IssueCommentController : Controller
    {
        private readonly IIssueCommentService _comments;
        private readonly IIssueService _issues;

        public IssueCommentController(
            IIssueCommentService comments,
            IIssueService issues)
        {
            _comments = comments;
            _issues = issues;
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

        // GET /IssueComment/Index?issueId=5
        [HttpGet]
        public async Task<IActionResult> Index(int issueId)
        {
            if (issueId <= 0)
                return NotFound();

            // Match the permissions of the existing
            // Issue Details page.
            var issue = await _issues.GetByIdAsync(
                issueId, CurrentUserId);

            if (issue == null)
                return NotFound();

            // The Facade filters internal notes
            // according to the authenticated user.
            var comments = await _comments.GetByIssueAsync(
                issueId, CurrentUserId);

            ViewBag.IssueId = issue.Id;
            ViewBag.IssueTitle = issue.Title;

            return View(comments);
        }

        // POST /IssueComment/Add?issueId=5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(
            int issueId,
            [Bind("Content,IsInternal")] CommentCreateDto model)
        {
            if (issueId <= 0)
                return NotFound();

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] =
                    "Please enter a valid comment of " +
                    "1 to 2000 characters.";

                return RedirectToAction(
                    nameof(Index), new { issueId });
            }

            try
            {
                await _comments.AddAsync(
                    issueId, CurrentUserId, model);

                TempData["SuccessMessage"] =
                    model.IsInternal
                        ? "Internal note added successfully."
                        : "Comment added successfully.";

                return RedirectToAction(
                    nameof(Index), new { issueId });
            }
            catch (ValidationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;

                return RedirectToAction(
                    nameof(Index), new { issueId });
            }
        }
    }
}
