using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebClient.Hubs;
using WebClient.ItemList;
using WebClient.Key;
using WebClient.Models;

namespace WebClient.Controllers
{
    public class HomeController : Controller
    {
        private readonly PeoplesList _peoplesList = PeoplesList.GetPeoplesList;
        private readonly IHubContext<UpdateHub> _myHub;

        public HomeController(IHubContext<UpdateHub> hub)
        {
            _myHub = hub;
            MyKey.OnKeyChanged += KeyChanged;
            _peoplesList.OnUpdate += Update;
        }

        private void Update()
        {
            _myHub.Clients.All.SendAsync("Update");
        }

        private void KeyChanged()
        {
            if (MyKey.Key == Guid.Empty)
                HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        [Authorize]
        [HttpGet]
        public ActionResult Index()
        {
            List<People> list = _peoplesList.GetPeoples();
            return View(list);
        }

        [Authorize]
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [AutoValidateAntiforgeryToken]
        [HttpPost]
        [Authorize]
        public ActionResult Create(People value)
        {
            _peoplesList.AddPeople(value);
            return RedirectToAction("Index");
        }

        [Authorize]
        [HttpGet]
        public ActionResult Edit(People value)
        {
            return View(value);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Guid id, People value)
        {
            _peoplesList.EditPeople(value);
            return RedirectToAction("Index");
        }

        [Authorize]
        [HttpGet]
        public ActionResult Delete(People value)
        {
            return View(value);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(Guid id)
        {
            _peoplesList.DeletePeople(id);
            return RedirectToAction("Index");
        }
    }
}
