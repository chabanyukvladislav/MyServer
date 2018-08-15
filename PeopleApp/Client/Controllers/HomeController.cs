using System;
using Client.ItemList;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;
using PeopleApp.Models;

namespace Client.Controllers
{
    public class HomeController : Controller
    {
        private const string MyHubAddress = "http://localhost:3121/Update/";
        private readonly PeoplesList _peoplesList = PeoplesList.GetPeoplesList;
        private readonly HubConnection _hub;

        public HomeController()
        {
            _peoplesList.OnPeopleChanged += PeopleChanged;
            _hub = new HubConnectionBuilder().WithUrl(MyHubAddress).Build();
            StartHub();
        }

        private void PeopleChanged()
        {
            _hub.InvokeAsync("update");
        }

        private async void StartHub()
        {
            await _hub.StartAsync();
        }

        public ActionResult Index()
        {
            return View(_peoplesList.GetPeoples());
        }

        public ActionResult Create()
        {
            return View();
        }

        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public ActionResult Create(People value)
        {
            _peoplesList.AddPeople(value);
            return RedirectToAction("Index");
        }

        public ActionResult Edit(People value)
        {
            return View(value);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Guid id, People value)
        {
            _peoplesList.EditPeople(value);
            return RedirectToAction("Index");
        }

        public ActionResult Delete(People value)
        {
            return View(value);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(Guid id)
        {
            _peoplesList.DeletePeople(id);
            return RedirectToAction("Index");
        }
    }
}
