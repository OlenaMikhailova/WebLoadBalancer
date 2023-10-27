using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebLoadBalancer.Models;
using WebLoadBalancer.ViewModels;
using BCrypt.Net;

namespace WebLoadBalancer.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationContext _context;

        public AccountController(ApplicationContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                web_user user = await _context.Users.FirstOrDefaultAsync(u => u.email == model.email);
                if (user != null && BCrypt.Net.BCrypt.Verify(model.user_password, user.user_password))
                {
                    await Authenticate(user);
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Incorrect login and (or) password");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                web_user user = await _context.Users.FirstOrDefaultAsync(u => u.email == model.email);
                if (user == null)
                {
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.user_password);
                    user = new web_user { email = model.email, user_password = hashedPassword, username = model.username };
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    await Authenticate(user);

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "A user with this email address already exists");
            }
            return View(model);
        }

        private async Task Authenticate(web_user user)
        {
            var claim = new List<Claim>()
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.username),
            };

            var claimsIdentity = new ClaimsIdentity(claim, CookieAuthenticationDefaults.AuthenticationScheme);


            var identity = new ClaimsIdentity(claim, CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);
            var authProperties = new AuthenticationProperties
            {
               /* IsPersistent = true*/ // Налаштуйте це за потребою
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}
