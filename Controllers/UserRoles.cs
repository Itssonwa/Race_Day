using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RaceDay.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
namespace RaceDay.Controllers
{
    [Authorize(Roles = "Organiser")]
    public class UserRoles : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserRoles(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager
            )

        {
            _userManager = userManager;
            _roleManager = roleManager;
        }


        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRolesViewModel = new List<UserRolesViewModel>();
            foreach (var user in users)
            {
                var thisViewModel = new UserRolesViewModel
                    {
                    UserID = user.Id,
                    Email = user.Email ?? "",
                    UserName = user.UserName ?? "",
                    Roles = await _userManager.GetRolesAsync(user)
                };
                userRolesViewModel.Add(thisViewModel);

            }
            return View(userRolesViewModel);
        }
    }
}