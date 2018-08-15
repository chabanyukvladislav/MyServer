﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Primitives;
using PeopleApp.DatabaseContext;
using PeopleApp.Hubs;
using PeopleApp.Models;
using PeopleApp.Users;

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
        public IEnumerable<People> Get()
        {
            try
            {
                Request.Query.TryGetValue("token", out StringValues token);
                if (Guid.TryParse(token, out Guid key) && ListUsers.GuidList.IsLogin(key))
                    return _context.Peoples;
                return new List<People>();
            }
            catch (Exception)
            {
                return new List<People>();
            }
        }

        [HttpGet("{id}")]
        public People Get(Guid id)
        {
            try
            {
                Request.Query.TryGetValue("token", out StringValues token);
                if (Guid.TryParse(token, out Guid key) && ListUsers.GuidList.IsLogin(key))
                    return _context.Peoples.FirstOrDefault(people => people.Id == id);
                return new People();
            }
            catch (Exception)
            {
                return new People();
            }
        }

        [HttpPost]
        public void Post([FromBody]People value)
        {
            Request.Query.TryGetValue("token", out StringValues token);
            if (!Guid.TryParse(token, out Guid key) || !ListUsers.GuidList.IsLogin(key))
                return;
            try
            {
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
        public void Delete(Guid id)
        {
            Request.Query.TryGetValue("token", out StringValues token);
            if (!Guid.TryParse(token, out Guid key) || !ListUsers.GuidList.IsLogin(key))
                return;
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
