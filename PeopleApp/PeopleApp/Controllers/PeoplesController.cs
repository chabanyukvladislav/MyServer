using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using PeopleApp.DatabaseContext;
using PeopleApp.Models;

namespace PeopleApp.Controllers
{
    [Route("api/[controller]")]
    public class PeoplesController : Controller
    {
        private readonly Context _context;

        public PeoplesController(Context context)
        {
            _context = context;
        }

        [HttpGet]
        public IEnumerable<People> Get()
        {
            return _context.Peoples;
        }

        [HttpPost]
        public void Post([FromBody]People value)
        {
            if (value.Id == Guid.Empty)
            {
                _context.Peoples.Add(value);
                _context.SaveChanges();
                return;
            }
            People people = _context.Peoples.Find(value.Id);
            if (people == null)
                return;
            people.Name = value.Name;
            people.Phone = value.Phone;
            people.Surname = value.Surname;
            _context.SaveChanges();
        }

        [HttpDelete("{id}")]
        public void Delete(Guid id)
        {
            _context.Peoples.Remove(_context.Peoples.Find(id));
            _context.SaveChanges();
        }
    }
}
