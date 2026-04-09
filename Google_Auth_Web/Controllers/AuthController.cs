using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Google_Auth_Web.Services;
using Google_Auth_Web.Models;
using QRCoder;

namespace Google_Auth_Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApiService _api;

        public AuthController(ApiService api)
        {
            _api = api;
        }

        // ================= REGISTER =================
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            var response = await _api.RegisterAsync(model);
            var result = JsonSerializer.Deserialize<JsonElement>(response);

            var qrUrl = result.GetProperty("qrCodeUrl").GetString();

            // ✅ Store everything for next step
            TempData["QR"] = qrUrl;
            TempData["Username"] = model.Username;
            TempData["Password"] = model.Password;

            return RedirectToAction("QRCode");
        }

        // ================= QR PAGE =================
        //public IActionResult QRCode()
        //{
        //    ViewBag.QR = TempData["QR"];

        //    //  Keep values for next request
        //    TempData.Keep("Username");
        //    TempData.Keep("Password");

        //    return View();
        //}
        //public IActionResult QRCode()
        //{
        //    var qr = TempData["QR"]?.ToString();

        //    if (string.IsNullOrEmpty(qr))
        //    {
        //        return Content("QR not found ❌"); // debug
        //    }

        //    ViewBag.QR = qr;

        //    TempData.Keep("QR"); // 🔥 IMPORTANT

        //    return View();
        //}
        public IActionResult QRCode()
        {
            var qrText = TempData["QR"]?.ToString();

            if (string.IsNullOrEmpty(qrText))
                return Content("QR not found ❌");

            // Generate QR
            using var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);

            var qrCode = new PngByteQRCode(qrData);
            byte[] qrBytes = qrCode.GetGraphic(10);

            string base64 = Convert.ToBase64String(qrBytes);

            ViewBag.QRImage = $"data:image/png;base64,{base64}";

            return View();
        }

        // ================= LOGIN =================
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model)
        {
            var response = await _api.LoginAsync(new
            {
                username = model.Username,
                password = model.Password
            });

            var result = JsonSerializer.Deserialize<JsonElement>(response);

            if (result.TryGetProperty("mfaRequired", out var mfa))
            {
                TempData["Username"] = model.Username;
                return RedirectToAction("VerifyOtp");
            }

            TempData["Token"] = result.GetProperty("token").GetString();
            return RedirectToAction("Success");
        }

        // ================= OTP =================
        public IActionResult VerifyOtp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOtp(OtpVM model)
        {
            var username = TempData["Username"]?.ToString();

            var response = await _api.VerifyOtpAsync(new
            {
                username = username,
                otpCode = model.OtpCode
            });

            var result = JsonSerializer.Deserialize<JsonElement>(response);

            if (result.TryGetProperty("token", out var token))
            {
                TempData["Token"] = token.GetString();
                return RedirectToAction("Success");
            }

            ViewBag.Error = "Invalid OTP";

            TempData.Keep("Username"); // keep for retry

            return View();
        }

        public IActionResult Success()
        {
            return Content("Login Successful 🎉");
        }
    }
}
