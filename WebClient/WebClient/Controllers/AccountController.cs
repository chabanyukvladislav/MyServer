using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using WebClient.Models;
using WebClient.Services;

namespace WebClient.Controllers
{
    public class AccountController : Controller
    {
        private const string ServerAddress = "http://192.168.1.19:1919/api/account";
        private readonly HttpClient _client;
        private HttpResponseMessage _response;

        public AccountController()
        {
            _client = new HttpClient();
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Login()
        {
            bool isCode = Request.Query.TryGetValue("code", out StringValues code);
            if (!isCode)
                return Content("Error");
            HttpContent data = new StringContent("{\"code\": \"" + code + "\"}", Encoding.UTF8, "application/json");
            _response = _client.PostAsync(ServerAddress, data).Result;
            string token = _response.Content.ReadAsStringAsync().Result;
            AddCookie(token);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<ActionResult> Logout()
        {
            string userId = User.Claims.FirstOrDefault(el => el.Type == "UserId")?.Value;
            if (userId == null)
                return RedirectToAction("Index", "Home");
            _client.DefaultRequestHeaders.Add("UserId", userId);
            if (bool.Parse(await (await _client.GetAsync(ServerAddress)).Content.ReadAsStringAsync()))
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        private async void AddCookie(string token)
        {
            JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            if (!(jwtSecurityTokenHandler.ReadToken(token) is JwtSecurityToken jwtSecurityToken))
                return;
            User user = GetUser(jwtSecurityToken);
            List<Claim> listClaim = new List<Claim> { new Claim("UserId", user.UserId) };
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(listClaim, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
            DataStore.UserId = user.UserId;
        }

        private User GetUser(JwtSecurityToken security)
        {
            string sub = security.Claims.FirstOrDefault(el => el.Type == "UserId")?.Value;
            string name = security.Claims.FirstOrDefault(el => el.Type == "Name")?.Value;
            string nickname = security.Claims.FirstOrDefault(el => el.Type == "Nickname")?.Value;
            string picture = security.Claims.FirstOrDefault(el => el.Type == "Picture")?.Value;
            User user = new User() { UserId = sub, Name = name, Nickname = nickname, Picture = picture };
            return user;
        }
    }
}
