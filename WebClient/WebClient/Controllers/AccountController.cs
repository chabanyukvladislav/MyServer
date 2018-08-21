using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace WebClient.Controllers
{
    public class AccountController : Controller
    {
        //private const string AuthorizeServer = "https://vlad191100.eu.auth0.com/authorize";
        //private const string RedirectUrl = "http://192.168.1.19:1920/Account/Login";
        private const string ServerAddress = "http://192.168.1.19:1919/api/account";
        //private const string ClientId = "Y2uaVJba1Ei6mYa7EaZiLajzlwOTo6jl";


        //[HttpGet]
        //public ActionResult Index()
        //{
        //    return Content("<a href=\"" + AuthorizeServer + "?response_type=code&client_id=" + ClientId + "&redirect_uri=" + RedirectUrl + "&scope=openid%20email\">Login</a>", "text/html");
        //}

        [HttpGet]
        public ActionResult Login()
        {
            bool isCode = Request.Query.TryGetValue("code", out StringValues code);
            if (!isCode)
                return Content("Error");
            HttpClient client = new HttpClient();
            HttpContent data = new StringContent("{\"code\": \"" + code + "\"}", Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PostAsync(ServerAddress, data).Result;
            string email = response.Content.ReadAsStringAsync().Result;
            return RedirectToAction("Index", "Home");
        }
    }
}
