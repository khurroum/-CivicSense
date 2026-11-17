using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using CivicSense.Entities;
using CivicSense.Facade.DTOs;
using CivicSense.Facade.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CivicSense.Web.UI.Controllers
{
    [Authorize(Policy = PermissionCodes.ManageAccess)]
    public class AccessAdminController : Controller
    {
        private readonly IAccessAdminService _access;

        public AccessAdminController(
            IAccessAdminService access)
        {
            _access = access;
        }

        private int CurrentUserId
        {
            get
            {
                string? value = User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

                if (!int.TryParse(value, out int id) ||
                    id <= 0)
                {
                    throw new UnauthorizedAccessException(
                        "Invalid authentication session.");
                }

                return id;
            }
        }

        // GET /AccessAdmin/Users
        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users = await _access.GetUsersAsync(
                CurrentUserId);

            return View(users);
        }

        // GET /AccessAdmin/Groups
        [HttpGet]
        public async Task<IActionResult> Groups()
        {
            var groups = await _access.GetGroupsAsync(
                CurrentUserId);

            return View(groups);
        }

        // GET /AccessAdmin/EditGroup/0
        // GET /AccessAdmin/EditGroup/5
        [HttpGet]
        public async Task<IActionResult> EditGroup(
            [FromRoute] int id = 0)
        {
            var page = await _access.GetGroupPageAsync(
                id, CurrentUserId);

            return View(page);
        }

        // POST /AccessAdmin/EditGroup/0
        // POST /AccessAdmin/EditGroup/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditGroup(
            [FromRoute] int id,
            [Bind(Prefix = "Form")] GroupAdminInputDto model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] =
                    "Please check the group name and selected permissions.";

                return RedirectToAction(
                    nameof(EditGroup), new { id });
            }

            try
            {
                int savedId = await _access.SaveGroupAsync(
                    id, model, CurrentUserId);

                TempData["SuccessMessage"] =
                    id == 0
                        ? "Permission group created successfully."
                        : "Permission group updated successfully.";

                return RedirectToAction(
                    nameof(EditGroup),
                    new { id = savedId });
            }
            catch (ValidationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["ErrorMessage"] =
                    "Another request changed this group. " +
                    "Review the latest values before saving again.";
            }

            return RedirectToAction(
                nameof(EditGroup), new { id });
        }

        // GET /AccessAdmin/EditUser/0
        // GET /AccessAdmin/EditUser/5
        [HttpGet]
        public async Task<IActionResult> EditUser(
            [FromRoute] int id = 0)
        {
            var page = await _access.GetUserPageAsync(
                id, CurrentUserId);

            return View(page);
        }

        // POST /AccessAdmin/EditUser/0
        // POST /AccessAdmin/EditUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(
            [FromRoute] int id,
            [Bind(Prefix = "Form")] UserAdminInputDto model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] =
                    "Please check the account details, " +
                    "password requirements and permissions.";

                return RedirectToAction(
                    nameof(EditUser), new { id });
            }

            try
            {
                int savedId = await _access.SaveUserAsync(
                    id, model, CurrentUserId);

                TempData["SuccessMessage"] =
                    id == 0
                        ? "User account created successfully."
                        : "User account and permissions updated.";

                return RedirectToAction(
                    nameof(EditUser),
                    new { id = savedId });
            }
            catch (ValidationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["ErrorMessage"] =
                    "Another request changed this account. " +
                    "Review the latest values before saving again.";
            }

            return RedirectToAction(
                nameof(EditUser), new { id });
        }
    }
}