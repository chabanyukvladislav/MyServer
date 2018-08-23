using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebClient.Collections;
using WebClient.Models;

namespace WebClient.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        [Authorize]
        public ActionResult Index()
        {
            string userId = User.Claims.FirstOrDefault(el => el.Type == "UserId")?.Value ?? "";
            if (!PhonesCollection.PeoplesListDictionary.ContainsKey(userId))
            {
                return RedirectToAction("Relogin", "Account");
            }

            while (PhonesCollection.PeoplesListDictionary[userId].IsChanged)
            {
                Thread.Sleep(10);
            }
            List<People> list = PhonesCollection.PeoplesListDictionary[userId].GetCollection() ?? new List<People>();
            return View(list);
        }

        [HttpGet]
        [Authorize]
        public ActionResult Create()
        {
            return View();
        }

        [AutoValidateAntiforgeryToken]
        [HttpPost]
        [Authorize]
        public ActionResult Create(People value)
        {
            string userId = User.Claims.FirstOrDefault(el => el.Type == "UserId")?.Value ?? "";
            if (!PhonesCollection.PeoplesListDictionary.ContainsKey(userId))
            {
                return RedirectToAction("Relogin", "Account");
            }
            PhonesCollection.PeoplesListDictionary[userId].AddPhone(value);
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize]
        public ActionResult Edit(People value)
        {
            return View(value);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Edit(Guid id, People value)
        {
            string userId = User.Claims.FirstOrDefault(el => el.Type == "UserId")?.Value ?? "";
            if (!PhonesCollection.PeoplesListDictionary.ContainsKey(userId))
            {
                return RedirectToAction("Relogin", "Account");
            }
            PhonesCollection.PeoplesListDictionary[userId].EditPhone(value);
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize]
        public ActionResult Delete(People value)
        {
            return View(value);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Delete(Guid id)
        {
            string userId = User.Claims.FirstOrDefault(el => el.Type == "UserId")?.Value ?? "";
            if (!PhonesCollection.PeoplesListDictionary.ContainsKey(userId))
            {
                return RedirectToAction("Relogin", "Account");
            }
            PhonesCollection.PeoplesListDictionary[userId].RemovePhone(id);
            return RedirectToAction("Index");
        }
    }
}
