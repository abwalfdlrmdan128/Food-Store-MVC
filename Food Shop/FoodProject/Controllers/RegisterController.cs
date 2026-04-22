using FoodProject.Data;
using FoodProject.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoodProject.Controllers
{

    public class RegisterController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        Context context = new Context();

        public RegisterController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(UserSignUpViewModel p)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.Users
                    .Where(u => u.Email == p.Mail)
                    .ToListAsync();

                if (existingUser.Count > 0)
                {
                    ModelState.AddModelError(nameof(p.Mail), "This email is already in use.");
                    return View(p);
                }

                AppUser user = new AppUser()
                {
                    Email = p.Mail,
                    UserName = p.UserName,
                    NameSurname = p.NameSurname
                };

                var result = await _userManager.CreateAsync(user, p.Password);

                if (result.Succeeded)
                {
                    const string DEFAULT_ROLE = "User";

                    if (!await _roleManager.RoleExistsAsync(DEFAULT_ROLE))
                    {
                        await _roleManager.CreateAsync(new AppRole { Name = DEFAULT_ROLE });
                    }

                    await _userManager.AddToRoleAsync(user, DEFAULT_ROLE);

                    return RedirectToAction("Index", "Login");
                }
                else
                {
                    foreach (var item in result.Errors)
                    {
                        if (item.Code.Contains("Password"))
                            ModelState.AddModelError(nameof(p.Password), item.Description);
                        else if (item.Code.Contains("UserName"))
                            ModelState.AddModelError(nameof(p.UserName), item.Description);
                        else
                            ModelState.AddModelError("", item.Description); // Errors 
                    }
                }
            }
            return View(p);
        }
    }
}
