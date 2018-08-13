using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Client.Key;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PeopleApp.Models;

namespace Client.Controllers
{
    public class HomeController : Controller
    {
        private string _serverAddress = "http://localhost:6881/api/peoples/" + MyKey.Key;

        public HomeController()
        {
            MyKey.OnKeyChanged += ChangeKey;
        }

        private void ChangeKey()
        {
            _serverAddress = "http://localhost:6881/api/peoples/" + MyKey.Key;
        }

        public ActionResult Index()
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = client.GetAsync(_serverAddress).Result;
            List<People> data = JsonConvert.DeserializeObject<List<People>>(response.Content.ReadAsStringAsync().Result);
            return View(data ?? new List<People>());
        }

        public ActionResult Create()
        {
            return View();
        }

        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public ActionResult Create(People value)
        {
            HttpClient client = new HttpClient();
            string json = JsonConvert.SerializeObject(value);
            int index = json.IndexOf("null", StringComparison.Ordinal);
            while (index != -1)
            {
                int i = json.Remove(index - 3).LastIndexOf("\"", StringComparison.Ordinal);
                string str1 = json.Remove(0, i + 1);
                string str2 = str1.Remove(str1.IndexOf("null", StringComparison.Ordinal) - 2);
                json = json.Remove(index - 3 - str2.Length, 8 + str2.Length);
                index = json.IndexOf("null", StringComparison.Ordinal);
            }
            StringContent data = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PostAsync(_serverAddress, data).Result;
            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");
            else
                return View(value);
        }

        public ActionResult Edit(People value)
        {
            return View(value);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Guid id, People value)
        {
            HttpClient client = new HttpClient();
            string json = JsonConvert.SerializeObject(value);
            int index = json.IndexOf("null", StringComparison.Ordinal);
            while (index != -1)
            {
                int i = json.Remove(index - 3).LastIndexOf("\"", StringComparison.Ordinal);
                string str1 = json.Remove(0, i + 1);
                string str2 = str1.Remove(str1.IndexOf("null", StringComparison.Ordinal) - 2);
                json = json.Remove(index - 3 - str2.Length, 8 + str2.Length);
                index = json.IndexOf("null", StringComparison.Ordinal);
            }
            StringContent data = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PostAsync(_serverAddress, data).Result;
            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");
            else
                return View(value);
        }

        public ActionResult Delete(People value)
        {
            return View(value);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(Guid id)
        {
            HttpClient client = new HttpClient();
            StringContent data = new StringContent("{\"id\": \"" + id + "\"}}", Encoding.UTF8, "application/json");
            client.PutAsync(_serverAddress, data).Wait();
            return RedirectToAction("Index");
        }
    }
}
