using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebClient.Key;
using WebClient.Models;

namespace WebClient.Controllers
{
    public class AccountController : Controller
    {
        private const string ServerAddress = "http://localhost/api/account/";

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public async Task<ActionResult> Login(User user)
        {
            try
            {
                HttpClient client = new HttpClient();
                string json = JsonConvert.SerializeObject(user);
                StringContent data = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = client.PostAsync(ServerAddress, data).Result;
                string key = response.Content.ReadAsStringAsync().Result.Trim('"');
                Guid guid = Guid.Parse(key);
                if (guid != Guid.Empty)
                {
                    MyKey.Key = guid;
                    await Authenticate(guid);
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Incorrect login or password");
                return View(user);
            }
            catch (Exception)
            {
                return View(user);
            }
        }

        private async Task Authenticate(Guid guid)
        {
            List<Claim> claims = new List<Claim> { new Claim(ClaimsIdentity.DefaultNameClaimType, guid.ToString()) };
            ClaimsIdentity identity = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        }
    }
}