using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebClient.Collections;
using WebClient.Hubs;
using WebClient.Models;
using WebClient.Services;

namespace WebClient.Controllers
{
    public class HomeController : Controller
    {
        private readonly PhonesCollection _peoplesList;
        private readonly IHubContext<UpdateHub> _myHub;

        public HomeController(IHubContext<UpdateHub> hub)
        {
            _peoplesList = PhonesCollection.GetPhonesCollection;
            _myHub = hub;
            _peoplesList.CollectionChanged += Update;
        }

        private void Update(object sender, NotifyCollectionChangedEventArgs e)
        {
            _myHub.Clients.All.SendAsync("Update");
        }

        [HttpGet]
        [Authorize]
        public ActionResult Index()
        {
            //DataStore.UserId = User.Claims.FirstOrDefault(el => el.Type == "UserId")?.Value ?? "";
            //_peoplesList.UpdateCollection();
            while (_peoplesList.IsChanged)
            {
                Thread.Sleep(10);
            }
            List<People> list = _peoplesList.GetCollection();
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
            //DataStore.UserId = User.Claims.FirstOrDefault(el => el.Type == "UserId")?.Value ?? "";
            _peoplesList.AddPhone(value);
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
            //DataStore.UserId = User.Claims.FirstOrDefault(el => el.Type == "UserId")?.Value ?? "";
            _peoplesList.EditPhone(value);
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
            //DataStore.UserId = User.Claims.FirstOrDefault(el => el.Type == "UserId")?.Value ?? "";
            _peoplesList.RemovePhone(id);
            return RedirectToAction("Index");
        }
    }
}
