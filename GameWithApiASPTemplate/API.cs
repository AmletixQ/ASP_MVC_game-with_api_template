using System;
using System.Text;
using System.Text.Json;

namespace GameWithApiASPTemplate
{
    public class API
    {
        private readonly HttpClient _httpClient;

        public API()
        {
            string apiKey = LoadApiKeyFromEnv();

            this._httpClient = new HttpClient();
            this._httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);
            this._httpClient.BaseAddress = new System.Uri("http://158.160.104.26:9001");
        }

        /// <summary>
        /// Загружает API ключ из .env файла
        /// </summary>
        private static string LoadApiKeyFromEnv()
        {
            string envPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env");

            if (!File.Exists(envPath))
                throw new FileNotFoundException(".env файл не найден. Создайте файл .env с содержимым: API_KEY=ваш_ключ");

            var lines = File.ReadAllLines(envPath);
            foreach (var line in lines)
                if (line.StartsWith("API_KEY="))
                    return line.Substring(8).Trim();

            throw new Exception("API_KEY не найден в .env файле");
        }

        /// <summary>
        /// Проверка доступности API
        /// </summary>
        /// <returns>true - если доступен, false - если недоступен</returns>
        public async Task<bool> Ping()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/ping");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<LoginResponse?> Login(string email, string password)
        {
            var loginData = new { email = email, password = password };
            var json = JsonSerializer.Serialize(loginData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/account/login", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Ошибка входа: {responseContent}");

            return JsonSerializer.Deserialize<LoginResponse>(responseContent);
        }

        public async Task<Game[]?> GetMyGames()
        {
            var response = await _httpClient.GetAsync("/api/games/my");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Ошибка получения игр: {responseContent}");

            return JsonSerializer.Deserialize<Game[]>(responseContent);
        }

        public async Task<PointsResponse?> GetPlayerPoints(string playerId)
        {
            var response = await _httpClient.GetAsync($"/api/users/{playerId}/points");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Ошибка получения баллов: {responseContent}");

            return JsonSerializer.Deserialize<PointsResponse>(responseContent);
        }

        public async Task<bool> AddPointsAsync(string userId, string gameId, int amount)
        {
            var pointsData = new { amount = amount };
            var json = JsonSerializer.Serialize(pointsData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"/api/users/{userId}/games/{gameId}/points", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Ошибка добавления баллов: {responseContent}");
            }

            return true;
        }
    }

    public class PointsResponse
    {
        public int points { get; set; }
    }

    public class Game
    {
        public int gameId { get; set; }
        public string gameTitle { get; set; }
        public string gamePublicationDate { get; set; }
    }

    public class LoginResponse
    {
        public string userId { get; set; }
    }
}
