using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using PeopleApp.DatabaseContext;
using PeopleApp.Models;
using PeopleApp.Users;

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

        [HttpGet("{key}")]
        public IEnumerable<People> Get(Guid key)
        {
            try
            {
                if (ListUsers.GuidList.IsLogin(key))
                    return _context.Peoples;
                return new List<People>();
            }
            catch (Exception)
            {
                return new List<People>();
            }
        }

        [HttpPost("{key}")]
        public void Post([FromBody]People value, Guid key)
        {
            if (!ListUsers.GuidList.IsLogin(key))
                return;
            try
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
            catch (Exception)
            {
                // ignored
            }
        }

        [HttpPut("{key}")]
        public void Delete([FromBody]People value, Guid key)
        {
            if (!ListUsers.GuidList.IsLogin(key))
                return;
            try
            {
                _context.Peoples.Remove(_context.Peoples.Find(value.Id));
                _context.SaveChanges();
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
