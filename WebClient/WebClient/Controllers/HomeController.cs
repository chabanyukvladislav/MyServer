using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebClient.Collections;
using WebClient.Hubs;
using WebClient.Models;

namespace WebClient.Controllers
{
    public class HomeController : Controller
    {
        private readonly PhonesCollection _peoplesList = PhonesCollection.GetPhonesCollection;
        private readonly IHubContext<UpdateHub> _myHub;

        public HomeController(IHubContext<UpdateHub> hub)
        {
            _myHub = hub;
            _peoplesList.CollectionChanged += Update;
        }

        private void Update(object sender, NotifyCollectionChangedEventArgs e)
        {
            _myHub.Clients.All.SendAsync("Update");
        }

        [HttpGet]
        public ActionResult Index()
        {
            while (_peoplesList.IsChanged)
            {
                Thread.Sleep(10);
            }
            List<People> list = _peoplesList.GetCollection();
            return View(list);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public ActionResult Create(People value)
        {
            _peoplesList.AddPhone(value);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Edit(People value)
        {
            return View(value);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Guid id, People value)
        {
            _peoplesList.EditPhone(value);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Delete(People value)
        {
            return View(value);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(Guid id)
        {
            _peoplesList.RemovePhone(id);
            return RedirectToAction("Index");
        }
    }
}
