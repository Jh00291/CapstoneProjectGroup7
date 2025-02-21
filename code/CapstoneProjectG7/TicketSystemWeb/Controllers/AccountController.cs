using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TicketSystemWeb.Models;
using TicketSystemWeb.Models.Employee;

namespace TicketSystemWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<Employee> _signInManager;
        private readonly UserManager<Employee> _userManager;

        public AccountController(SignInManager<Employee> signInManager, UserManager<Employee> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Invalid username or password.");
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
