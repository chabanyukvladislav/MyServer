using System;
using System.Net.Http;
using System.Text;
using Client.Key;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PeopleApp.Models;

namespace Client.Controllers
{
    public class AccountController : Controller
    {
        private const string ServerAddress = "http://localhost:6881/api/account/";

        public ActionResult Login()
        {
            return View();
        }

        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public ActionResult Login(User user)
        {
            try
            {
                HttpClient client = new HttpClient();
                string json = JsonConvert.SerializeObject(user);
                StringContent data = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = client.PostAsync(ServerAddress, data).Result;
                string key = response.Content.ReadAsStringAsync().Result.Trim('"');
                MyKey.Key = Guid.Parse(key);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception)
            {
                return View();
            }
        }
    }
}