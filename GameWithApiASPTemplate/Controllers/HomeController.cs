using Microsoft.AspNetCore.Mvc;
using GameWithApiASPTemplate;
using System.Text.Json;

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

            if (TempData["Games"] != null)
                ViewData["Games"] = JsonSerializer.Deserialize<Game[]>(TempData["Games"]?.ToString() ?? "[]");

            //try
            //{
            //    var games = await _api.GetMyGames() ?? Array.Empty<Game>();
            //    ViewData["Games"] = games;
            //}
            //catch (Exception ex)
            //{
            //    ViewData["Error"] = $"Ошибка загрузки игр: {ex.Message}";
            //    ViewData["Games"] = Array.Empty<Game>();
            //}

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
        public async Task<IActionResult> GetDevGames()
        {
            try
            {
                var games = await _api.GetMyGames() ?? [];
                TempData["Games"] = JsonSerializer.Serialize(games);
                TempData["Message"] = "Список игр успешно обновлён";
            }
            catch (Exception ex)
            {
                ViewData["Error"] = $"Ошибка загрузки игр: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> AddPoints(string gameId, string playerId, int amount)
        {
            if (string.IsNullOrWhiteSpace(gameId) || string.IsNullOrWhiteSpace(playerId) || amount <= 0)
            {
                TempData["Error"] = "Заполните все поля корректно";
                return RedirectToAction("Index");
            }

            try
            {
                await _api.AddPointsAsync(playerId, gameId, amount);
                TempData["Message"] = $"Успешно добавлено {amount} баллов игроку {playerId} в игру с ID {gameId}!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка добавления баллов: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}