using FraudFence.EntityModels.Dto;
using FraudFence.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace FraudFence.Web.Controllers
{
    [Authorize]
    public class AccountsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        public AccountsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient("UsersApi");
            _apiBaseUrl = configuration["AWS:ApiGateway:UserManagementApiUrl"]!;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromForm] RegistrationViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var dto = new RegistrationDTO(vm.Name, vm.Email, vm.Password);
            var response = await _httpClient.PostAsJsonAsync($"{_apiBaseUrl}/register", dto);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Registration failed. Please try again.");
                return View(vm);
            }

            // Automatically log the user in after registration
            return await Login(new LoginViewModel { Email = vm.Email, Password = vm.Password });
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromForm] LoginViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var loginDto = new LoginDTO { Email = vm.Email, Password = vm.Password };
            
            // Log the request body
            System.Diagnostics.Debug.WriteLine($"Sending login request to {_apiBaseUrl}/login with body: {JsonSerializer.Serialize(loginDto)}");

            var response = await _httpClient.PostAsJsonAsync($"{_apiBaseUrl}/login", loginDto);
            
            // Log the full response body
            var responseBody = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"Received login response. Status: {response.StatusCode}, Body: {responseBody}");

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(vm);
            }

            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);
            var idToken = tokenResponse.GetProperty("IdToken").GetString();

            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(idToken) as JwtSecurityToken;

            var claims = new List<Claim>(jsonToken!.Claims);
            var subClaim = claims.FirstOrDefault(c => c.Type == "sub");
            if (subClaim != null)
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, subClaim.Value));
            }
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, "cognito:groups");
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = false
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            var roles = claims.Where(c => c.Type == "cognito:groups").Select(c => c.Value).ToList();
            var area = roles.Contains("Admin") ? "Admin"
                       : roles.Contains("Publisher") ? "Publisher"
                       : roles.Contains("Reviewer") ? "Reviewer"
                       : "Consumer";

            return RedirectToAction("Index", "Home", new { area });
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
