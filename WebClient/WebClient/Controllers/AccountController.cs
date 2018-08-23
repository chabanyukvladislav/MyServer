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
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Primitives;
using WebClient.Collections;
using WebClient.Hubs;
using WebClient.Models;

namespace WebClient.Controllers
{
    public class AccountController : Controller
    {
        private const string ServerAddress = "http://185.247.21.82:9090/api/account";
        //private const string ServerAddress = "http://localhost:6881/api/account";
        private readonly IHubContext<UpdateHub> _myHub;
        private readonly HttpClient _client;
        //private readonly IDataStore _dataStore;
        private HttpResponseMessage _response;

        public AccountController(IHubContext<UpdateHub> hub)
        {
            _myHub = hub;
            //_dataStore = DataStore.GetDataStore;
            _client = new HttpClient();
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> Login()
        {
            bool isCode = Request.Query.TryGetValue("code", out StringValues code);
            if (!isCode)
                return RedirectToAction("Index");
            HttpContent data = new StringContent("{\"code\": \"" + code + "\"}", Encoding.UTF8, "application/json");
            _response = _client.PostAsync(ServerAddress, data).Result;
            string token = _response.Content.ReadAsStringAsync().Result;
            JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            if (!(jwtSecurityTokenHandler.ReadToken(token) is JwtSecurityToken jwtSecurityToken))
                return RedirectToAction("Index");
            User user = GetUser(jwtSecurityToken);
            await AddCookie(user);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<ActionResult> Logout()
        {
            string userId = User.Claims.FirstOrDefault(el => el.Type == "UserId")?.Value;
            if (userId == null)
                return RedirectToAction("Index", "Home");
            //_client.DefaultRequestHeaders.Add("UserId", userId);
            //HttpResponseMessage response = await _client.GetAsync(ServerAddress);
            //string result = await response.Content.ReadAsStringAsync();
            //if (!bool.Parse(result))
            //    return RedirectToAction("Index", "Home");
            PhonesCollection.PeoplesListDictionary.Remove(userId);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<ActionResult> Relogin()
        {
            string userId = User.Claims.FirstOrDefault(el => el.Type == "UserId")?.Value;
            if (userId == null)
                return RedirectToAction("Index");
            await AddCookie(userId);
            return RedirectToAction("Index","Home");
        }

        private async Task AddCookie(User user)
        {
            PhonesCollection.PeoplesListDictionary.Add(user.UserId, new PhonesCollection(user.UserId, _myHub));
            List<Claim> listClaim = new List<Claim> { new Claim("UserId", user.UserId) };
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(listClaim, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
        }
        private async Task AddCookie(string userId)
        {
            PhonesCollection.PeoplesListDictionary.Add(userId, new PhonesCollection(userId, _myHub));
            List<Claim> listClaim = new List<Claim> { new Claim("UserId", userId) };
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(listClaim, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
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
