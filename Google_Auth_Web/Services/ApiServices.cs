
using System.Net.Http;
using System.Text;
using System.Text.Json;
namespace Google_Auth_Web.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;

        public ApiService(HttpClient http)
        {
            _http = http;
            _http.BaseAddress = new Uri("http://localhost:5001/api/"); // your API URL
        }

        public async Task<string> RegisterAsync(object data)
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync("auth/register", content);
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> VerifyOtpAsync(object data)
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync("auth/verify-otp", content);

            return await response.Content.ReadAsStringAsync();
        }
        public async Task<string> LoginAsync(object data)
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync("auth/login", content);
            return await response.Content.ReadAsStringAsync();
        }
    }
}
