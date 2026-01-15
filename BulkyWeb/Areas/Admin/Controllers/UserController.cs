using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWorkRepository _unitOfWork;

        public UserController(
            ApplicationDbContext db,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IUnitOfWorkRepository unitOfWork)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Upsert(string? id)
        {
            var userVM = new UserVM
            {
                CompanyList = _unitOfWork.Company.GetAll().Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                }),
                RoleList = GetRoleListForCurrentUser()
            };

            if (string.IsNullOrEmpty(id))
            {
                // Create new user
                userVM.ApplicationUser = new ApplicationUser();
                return View(userVM);
            }
            else
            {
                // Edit existing user
                var user = _db.ApplicationUsers
                    .Include(u => u.Company)
                    .FirstOrDefault(u => u.Id == id);

                if (user == null)
                {
                    return NotFound();
                }

                userVM.ApplicationUser = user;

                // Get current role
                var roles = await _userManager.GetRolesAsync(user);
                userVM.ApplicationUser.Role = roles.FirstOrDefault();

                return View(userVM);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(UserVM userVM, string? id)
        {
            if (!ModelState.IsValid)
            {
                userVM.CompanyList = _unitOfWork.Company.GetAll().Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                });
                userVM.RoleList = GetRoleListForCurrentUser();
                return View(userVM);
            }

            // Validate role permissions
            if (!CanManageRole(userVM.ApplicationUser.Role))
            {
                TempData["error"] = "You don't have permission to assign this role.";
                userVM.CompanyList = _unitOfWork.Company.GetAll().Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                });
                userVM.RoleList = GetRoleListForCurrentUser();
                return View(userVM);
            }

            if (string.IsNullOrEmpty(id))
            {
                // Create new user
                var user = new ApplicationUser
                {
                    UserName = userVM.ApplicationUser.Email,
                    Email = userVM.ApplicationUser.Email,
                    Name = userVM.ApplicationUser.Name,
                    PhoneNumber = userVM.ApplicationUser.PhoneNumber,
                    StreetAddress = userVM.ApplicationUser.StreetAddress,
                    City = userVM.ApplicationUser.City,
                    State = userVM.ApplicationUser.State,
                    PostalCode = userVM.ApplicationUser.PostalCode,
                    CompanyId = userVM.ApplicationUser.Role == SD.Role_Company ? userVM.ApplicationUser.CompanyId : null
                };

                var result = await _userManager.CreateAsync(user, userVM.ApplicationUser.PasswordHash);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(userVM.ApplicationUser.Role))
                    {
                        await _userManager.AddToRoleAsync(user, userVM.ApplicationUser.Role);
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(user, SD.Role_Customer);
                    }

                    TempData["success"] = "User created successfully";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            else
            {
                // Update existing user
                var userFromDb = _db.ApplicationUsers.FirstOrDefault(u => u.Id == id);

                if (userFromDb == null)
                {
                    return NotFound();
                }

                userFromDb.Name = userVM.ApplicationUser.Name;
                userFromDb.PhoneNumber = userVM.ApplicationUser.PhoneNumber;
                userFromDb.StreetAddress = userVM.ApplicationUser.StreetAddress;
                userFromDb.City = userVM.ApplicationUser.City;
                userFromDb.State = userVM.ApplicationUser.State;
                userFromDb.PostalCode = userVM.ApplicationUser.PostalCode;
                userFromDb.CompanyId = userVM.ApplicationUser.Role == SD.Role_Company ? userVM.ApplicationUser.CompanyId : null;

                _db.SaveChanges();

                // Update role
                var currentRoles = await _userManager.GetRolesAsync(userFromDb);
                var currentRole = currentRoles.FirstOrDefault();

                if (currentRole != userVM.ApplicationUser.Role)
                {
                    if (!string.IsNullOrEmpty(currentRole))
                    {
                        await _userManager.RemoveFromRoleAsync(userFromDb, currentRole);
                    }

                    await _userManager.AddToRoleAsync(userFromDb, userVM.ApplicationUser.Role);
                }

                TempData["success"] = "User updated successfully";
                return RedirectToAction(nameof(Index));
            }

            userVM.CompanyList = _unitOfWork.Company.GetAll().Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString()
            });
            userVM.RoleList = GetRoleListForCurrentUser();
            return View(userVM);
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var userList = _db.ApplicationUsers
                .Include(u => u.Company)
                .ToList();

            var userRoles = _db.UserRoles.ToList();
            var roles = _db.Roles.ToList();

            foreach (var user in userList)
            {
                var roleId = userRoles.FirstOrDefault(u => u.UserId == user.Id)?.RoleId;
                user.Role = roles.FirstOrDefault(r => r.Id == roleId)?.Name;

                if (user.Company == null)
                {
                    user.Company = new Company { Name = "" };
                }
            }

            return Json(new { data = userList });
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody] string id)
        {
            var userFromDb = _db.ApplicationUsers.FirstOrDefault(u => u.Id == id);

            if (userFromDb == null)
            {
                return Json(new { success = false, message = "Error while Locking/Unlocking" });
            }

            if (userFromDb.LockoutEnd != null && userFromDb.LockoutEnd > DateTime.Now)
            {
                // User is currently locked, unlock them
                userFromDb.LockoutEnd = DateTime.Now;
            }
            else
            {
                // Lock the user for 1000 years
                userFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
            }

            _db.SaveChanges();
            return Json(new { success = true, message = "Operation Successful" });
        }

        #endregion

        #region HELPER METHODS

        private IEnumerable<SelectListItem> GetRoleListForCurrentUser()
        {
            var roles = _roleManager.Roles.AsEnumerable();

            // If current user is Employee, exclude Admin and Employee roles
            if (User.IsInRole(SD.Role_Employee))
            {
                roles = roles.Where(r => r.Name != SD.Role_Admin && r.Name != SD.Role_Employee);
            }

            return roles.Select(r => new SelectListItem
            {
                Text = r.Name,
                Value = r.Name
            });
        }

        private bool CanManageRole(string roleName)
        {
            // Admin can manage all roles
            if (User.IsInRole(SD.Role_Admin))
            {
                return true;
            }

            // Employee cannot manage Admin and Employee roles
            if (User.IsInRole(SD.Role_Employee))
            {
                return roleName != SD.Role_Admin && roleName != SD.Role_Employee;
            }

            return false;
        }

        #endregion
    }
}
