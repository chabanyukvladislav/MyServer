using System;
using Client.ItemList;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;
using PeopleApp.Models;

namespace Client.Controllers
{
    public class HomeController : Controller
    {
        private const string HubAddress = "http://localhost:6881/Notification/";
        private const string MyHubAddress = "http://localhost:3121/Update/";
        private readonly PeoplesList _peoplesList = PeoplesList.GetPeoplesList;
        private readonly HubConnection _hubConnection;
        private readonly HubConnection _myHub;

        public HomeController()
        {
            _peoplesList.OnPeopleAdd += PeopleAdded;
            _peoplesList.OnPeopleDelete += PeopleDeleted;
            _hubConnection = new HubConnectionBuilder().WithUrl(HubAddress).Build();
            _myHub = new HubConnectionBuilder().WithUrl(MyHubAddress).Build();
            MyHub();
            StartHub();
        }

        private void PeopleDeleted(Guid obj)
        {
            _myHub.InvokeAsync("del", obj);
        }

        private void PeopleAdded(People obj)
        {
            _myHub.InvokeAsync("add", obj);
        }

        private async void StartHub()
        {
            await _hubConnection.StartAsync();
            _hubConnection.On<People>("Add", (value) =>
            {
                _peoplesList.AddPeople(value);
            });
            _hubConnection.On<People>("Edit", (value) =>
            {
                _peoplesList.EditPeople(value);
            });
            _hubConnection.On<People>("Delete", (value) =>
            {
                _peoplesList.DeletePeople(value.Id);
            });
        }
        private async void MyHub()
        {
            await _myHub.StartAsync();
        }

        public ActionResult Index()
        {
            return View(_peoplesList.Peoples);
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
