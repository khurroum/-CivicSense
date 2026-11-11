
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using CivicSense.Facade.DTOs;
using CivicSense.Facade.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CivicSense.Web.UI.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categories;

        public CategoryController(ICategoryService categories)
        {
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

        // GET: /Category
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            bool isAdmin = User.IsInRole("Admin");

            var categories = isAdmin
                ? await _categories.GetAllAsync()
                : await _categories.GetActiveAsync();

            return View(categories);
        }

        // GET: /Category/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var category = await _categories.GetByIdAsync(id);

            if (category == null)
                return NotFound();

            if (!category.IsActive &&
                !User.IsInRole("Admin"))
                return NotFound();

            return View(category);
        }

        // GET: /Category/Create
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View(new CategoryCreateDto());
        }

        // POST: /Category/Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CategoryCreateDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _categories.CreateAsync(
                    model, CurrentUserId);

                TempData["SuccessMessage"] =
                    "Category created successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError(
                    string.Empty, ex.Message);

                return View(model);
            }
        }

        // GET: /Category/Edit/5
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categories.GetByIdAsync(id);

            if (category == null)
                return NotFound();

            var model = new CategoryUpdateDto
            {
                Name = category.Name,
                Description = category.Description
            };

            ViewBag.CategoryId = category.Id;

            return View(model);
        }

        // POST: /Category/Edit/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, CategoryUpdateDto model)
        {
            ViewBag.CategoryId = id;

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                bool updated = await _categories.UpdateAsync(
                    id, model, CurrentUserId);

                if (!updated)
                    return NotFound();

                TempData["SuccessMessage"] =
                    "Category updated successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError(
                    string.Empty, ex.Message);

                return View(model);
            }
        }

        // POST: /Category/Deactivate/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            bool success = await _categories.DeactivateAsync(
                id, CurrentUserId);

            if (!success)
                return NotFound();

            TempData["SuccessMessage"] =
                "Category deactivated successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
