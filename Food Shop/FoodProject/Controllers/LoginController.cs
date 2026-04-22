using FoodProject.Data;
using FoodProject.Data.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FoodProject.Controllers
{
    [AllowAnonymous]
    //[Authorize(Roles = "Admin,Uye")]
    public class LoginController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        Context context = new Context();
        public LoginController(SignInManager<AppUser> signInManager)
        {
            _signInManager = signInManager;
        }

        //[AllowAnonymous] 
        [HttpGet] 
        public IActionResult Index()
        {
            return View();
        }


        //[AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Index(UserSignInViewModel p)
        {
            if (!ModelState.IsValid)
            {
              // (client-side validation)
                return View(p); 
            }

            var result = await _signInManager.PasswordSignInAsync(p.Username, p.Password, false, true);

            if (result.Succeeded)
            {
                var user = context.Users.FirstOrDefault(x => x.UserName == p.Username);
                if (user != null)
                {
                    var userRoleId = context.UserRoles
                        .Where(x => x.UserId == user.Id)
                        .Select(y => y.RoleId)
                        .FirstOrDefault();

                    var roleName = context.Roles
                        .Where(x => x.Id == userRoleId)
                        .Select(y => y.Name)
                        .FirstOrDefault();

                    if (roleName == "Admin")
                        return RedirectToAction("Statistics", "Chart");

                    return RedirectToAction("Index", "Default");
                }
            }
            ModelState.AddModelError(string.Empty, "incorrect UserName or PassWord");
            return View(p);
        }


        public IActionResult AccessDenied()
        {
            return View();
        }
        

        [HttpGet]
        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Category");

        }

    }
}
