using Microsoft.AspNetCore.Mvc;
using GameWithApiASPTemplate;

namespace GameWithApiASPTemplate.Controllers
{
    public class HomeController : Controller
    {
        private readonly API _api;

        public HomeController(API api)
        {
            _api = api;
        }

        private string? CurrentUserId => Request.Cookies["UserId"];

        public async Task<IActionResult> Index()
        {
            var userId = CurrentUserId;
            ViewData["IsLoggedIn"] = !string.IsNullOrEmpty(userId);
            ViewData["CurrentUserId"] = userId;

            if (TempData["Message"] != null)
                ViewData["Message"] = TempData["Message"]?.ToString();
            if (TempData["Error"] != null)
                ViewData["Error"] = TempData["Error"]?.ToString();

            if (!(bool)ViewData["IsLoggedIn"])
                return View();

            try
            {
                var games = await _api.GetMyGames() ?? [];
                ViewData["Games"] = games;
            }
            catch
            {
                ViewData["Games"] = new List<Game>();
            }

            return View();
        }

        public async Task<IActionResult> GetPoints(string playerId)
        {
            if (string.IsNullOrEmpty(CurrentUserId))
                return RedirectToAction("Login", "Account");

            try
            {
                var result = await _api.GetPlayerPoints(playerId);
                TempData["Message"] = $"Баллы игрока {playerId}: {result?.points ?? 0}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка получения баллов: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> AddPoints(string gameTitle, string playerId, int amount)
        {
            if (string.IsNullOrEmpty(CurrentUserId))
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(gameTitle) || string.IsNullOrWhiteSpace(playerId) || amount <= 0)
            {
                TempData["Error"] = "Заполните все поля корректно";
                return RedirectToAction("Index");
            }

            try
            {
                var games = await _api.GetMyGames();

                var foundGame = games?.FirstOrDefault(g =>
                    g.gameTitle != null &&
                    g.gameTitle.Trim().Equals(gameTitle.Trim(), StringComparison.OrdinalIgnoreCase));

                if (foundGame == null)
                {
                    TempData["Error"] = $"Игра с названием «{gameTitle}» не найдена.";
                    return RedirectToAction("Index");
                }

                await _api.AddPointsAsync(playerId, foundGame.gameId.ToString(), amount);

                TempData["Message"] = $"Успешно добавлено {amount} баллов игроку {playerId} в игру «{foundGame.gameTitle}»!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка добавления баллов: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}