
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using CivicSense.Entities;
using CivicSense.Facade.DTOs;
using CivicSense.Facade.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CivicSense.Web.UI.Controllers
{
    [Authorize]
    public class IssueController : Controller
    {
        private readonly IIssueService _issues;
        private readonly ICategoryService _categories;

        public IssueController(
            IIssueService issues,
            ICategoryService categories)
        {
            _issues = issues;
            _categories = categories;
        }

        private int CurrentUserId
        {
            get
            {
                string? value = User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

                if (!int.TryParse(value, out int userId))
                    throw new UnauthorizedAccessException(
                        "Invalid authentication session.");

                return userId;
            }
        }

        // GET: /Issue
        [HttpGet]
        [Authorize(Roles = "Resident")]
        public async Task<IActionResult> Index()
        {
            var issues = await _issues.GetMyIssuesAsync(
                CurrentUserId);

            return View(issues);
        }

        // GET: /Issue/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var issue = await _issues.GetByIdAsync(
                id, CurrentUserId);

            if (issue == null)
                return NotFound();

            return View(issue);
        }

        // GET: /Issue/Create
        [HttpGet]
        [Authorize(Roles = "Resident")]
        public async Task<IActionResult> Create()
        {
            await LoadCategoriesAsync();

            return View(new IssueCreateDto
            {
                IssueType = IssueType.PublicCivic
            });
        }

        // POST: /Issue/Create
        [HttpPost]
        [Authorize(Roles = "Resident")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            IssueCreateDto model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync(model.CategoryId);
                return View(model);
            }

            try
            {
                var created = await _issues.CreateAsync(
                    model, CurrentUserId);

                TempData["SuccessMessage"] =
                    "Issue submitted successfully.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = created.Id });
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError(
                    string.Empty, ex.Message);

                await LoadCategoriesAsync(model.CategoryId);
                return View(model);
            }
        }

        // GET: /Issue/Edit/5
        [HttpGet]
        [Authorize(Roles = "Resident")]
        public async Task<IActionResult> Edit(int id)
        {
            var issue = await _issues.GetByIdAsync(
                id, CurrentUserId);

            if (issue == null)
                return NotFound();

            if (issue.ReporterId != CurrentUserId)
                return Forbid();

            if (issue.Status != IssueStatus.Reported &&
                issue.Status != IssueStatus.NeedsInformation)
            {
                TempData["ErrorMessage"] =
                    "This issue can no longer be edited.";

                return RedirectToAction(
                    nameof(Details), new { id });
            }

            var model = new IssueUpdateDto
            {
                Title = issue.Title,
                Description = issue.Description,
                Address = issue.Address,
                CategoryId = issue.CategoryId,
                Latitude = issue.Latitude,
                Longitude = issue.Longitude,

                // Preserve the database version
                // when opening the Edit form.
                RowVersion = issue.RowVersion.ToArray()
            };

            ViewBag.IssueId = id;

            await LoadCategoriesAsync(model.CategoryId);

            return View(model);
        }

        // POST: /Issue/Edit/5
        [HttpPost]
        [Authorize(Roles = "Resident")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, IssueUpdateDto model)
        {
            ViewBag.IssueId = id;

            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync(model.CategoryId);
                return View(model);
            }

            try
            {
                bool success = await _issues.UpdateAsync(
                    id, model, CurrentUserId);

                if (!success)
                    return NotFound();

                TempData["SuccessMessage"] =
                    "Issue updated successfully.";

                return RedirectToAction(
                    nameof(Details), new { id });
            }
            catch (DbUpdateConcurrencyException)
            {
                // The database record has changed
                // since the Edit form was opened.
                TempData["ErrorMessage"] =
                    "This issue was updated by another user. " +
                    "Your changes were not saved. " +
                    "Please review the latest issue information.";

                return RedirectToAction(
                    nameof(Details), new { id });
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError(
                    string.Empty, ex.Message);

                await LoadCategoriesAsync(model.CategoryId);
                return View(model);
            }
        }

        // POST: /Issue/Archive/5
        [HttpPost]
        [Authorize(Roles = "Resident")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(int id)
        {
            try
            {
                bool success = await _issues.ArchiveAsync(
                    id, CurrentUserId);

                if (!success)
                    return NotFound();

                TempData["SuccessMessage"] =
                    "Issue archived successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (ValidationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;

                return RedirectToAction(
                    nameof(Details), new { id });
            }
        }

        private async Task LoadCategoriesAsync(
            int? selectedCategoryId = null)
        {
            var categories = await _categories.GetActiveAsync();

            ViewBag.Categories = new SelectList(
                categories,
                "Id",
                "Name",
                selectedCategoryId);
        }
    }
}
