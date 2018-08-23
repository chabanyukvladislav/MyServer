﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PeopleApp.Attributes;
using PeopleApp.DatabaseContext;
using PeopleApp.Hubs;
using PeopleApp.Models;

namespace PeopleApp.Controllers
{
    [Route("api/[controller]")]
    public class PeoplesController : Controller
    {
        private readonly Context _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public PeoplesController(Context context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpGet]
        [Authorized]
        public IEnumerable<People> Get()
        {
            try
            {
                string userId = HttpContext.Request.Headers["UserId"];
                return _context.Peoples.Where(el => el.User.Equals(_context.Users.FirstOrDefault(us => us.UserId == userId)));
            }
            catch (Exception)
            {
                return new List<People>();
            }
        }

        [HttpGet("{id}")]
        [Authorized]
        public People Get(Guid id)
        {
            try
            {
                string userId = HttpContext.Request.Headers["UserId"];
                return _context.Peoples.FirstOrDefault(people => people.Id == id && people.User.UserId == userId);
            }
            catch (Exception)
            {
                return null;
            }
        }

        [HttpPost]
        [Authorized]
        public void Post([FromBody]People value)
        {
            try
            {
                string userId = HttpContext.Request.Headers["UserId"];
                value.User = _context.Users.FirstOrDefault(user => user.UserId == userId);
                if (value.Id == Guid.Empty)
                {
                    _context.Peoples.Add(value);
                    _context.SaveChanges();
                    _hubContext.Clients.All.SendAsync("Add", value.Id);
                    return;
                }
                People people = _context.Peoples.Find(value.Id);
                if (people == null)
                    return;
                people.Name = value.Name;
                people.Phone = value.Phone;
                people.Surname = value.Surname;
                _context.SaveChanges();
                _hubContext.Clients.All.SendAsync("Edit", value.Id);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        [HttpDelete("{id}")]
        [Authorized]
        public void Delete(Guid id)
        {
            try
            {
                _context.Peoples.Remove(_context.Peoples.Find(id));
                _context.SaveChanges();
                _hubContext.Clients.All.SendAsync("Delete", id);
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
