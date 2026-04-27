using FoodProject.Data;
using FoodProject.Data.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FoodProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SettingsController : Controller
    {
        Context context = new Context();
        private readonly UserManager<AppUser> _userManager;

        public SettingsController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // var values = await _userManager.FindByNameAsync(User.Identity.Name);
            var userName = User.Identity.Name;
            var userID=context.Users.Where(x=>x.UserName== userName).Select(y=>y.Id).FirstOrDefault();
            var eMail= context.Users.Where(x => x.Id == userID).Select(y => y.Email).FirstOrDefault();
            var nameSurname= context.Users.Where(x => x.Id == userID).Select(y => y.NameSurname).FirstOrDefault();

            UserUpdateModel model = new UserUpdateModel();
            model.userid = userID;
            model.email = eMail;
            model.namesurname = nameSurname;
            model.username = userName;
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Index(UserUpdateModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // get current user
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            if (user == null)
                return NotFound();

            // update basic data
            user.NameSurname = model.namesurname;
            user.UserName = model.username;
            user.Email = model.email;

            // update password only if entered
            if (!string.IsNullOrEmpty(model.password))
            {
                user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, model.password);
            }

            // save changes
            IdentityResult result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }
            return RedirectToAction("Statistics", "Chart");
        }

    }
}
