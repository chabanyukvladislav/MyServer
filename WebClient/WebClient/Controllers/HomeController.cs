using System;
using Microsoft.AspNetCore.Mvc;
using WebClient.ItemList;
using WebClient.Models;

namespace WebClient.Controllers
{
    public class HomeController : Controller
    {
        private readonly PeoplesList _peoplesList = PeoplesList.GetPeoplesList;

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
            bool result = _peoplesList.AddPeople(value);
            while (result && !_peoplesList.IsChanged) { }
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
            bool result = _peoplesList.EditPeople(value);
            while (result && !_peoplesList.IsChanged) { }
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
            bool result = _peoplesList.DeletePeople(id);
            while (result && !_peoplesList.IsChanged) { }
            return RedirectToAction("Index");
        }
    }
}
