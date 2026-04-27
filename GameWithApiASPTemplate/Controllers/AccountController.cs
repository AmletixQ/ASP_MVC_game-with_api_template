using Microsoft.AspNetCore.Mvc;
using GameWithApiASPTemplate;

namespace GameWithApiASPTemplate.Controllers
{
    public class AccountController : Controller
    {
        private string? CurrentUserId => Request.Cookies["UserId"];

        private readonly API _api;

        public AccountController(API api)
        {
            _api = api;
        }

        public IActionResult Login()
        {
            if (Request.Cookies.ContainsKey("UserId"))
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewData["Error"] = "Введите email и пароль";
                return RedirectToAction("Login");
            }

            try
            {
                var response = await _api.Login(email, password);

                if (response?.userId != null)
                {
                    Response.Cookies.Append("UserId", response.userId, new CookieOptions
                    {
                        HttpOnly = true,
                        IsEssential = true,
                        Expires = DateTimeOffset.UtcNow.AddHours(8)
                    });

                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                ViewData["Error"] = $"Ошибка входа: {ex.Message}";
            }

            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            Response.Cookies.Delete("UserId");
            return RedirectToAction("Index", "Home");
        }

    }
}