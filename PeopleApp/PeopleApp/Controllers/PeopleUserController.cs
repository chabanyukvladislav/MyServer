using System;
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
    [Produces("application/json")]
    [Route("api/PeopleUser")]
    public class PeopleUserController : Controller
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly Context _context;

        public PeopleUserController(Context context, IHubContext<NotificationHub> hub)
        {
            _hubContext = hub;
            _context = context;
        }

        [HttpGet]
        [Authorized]
        public IEnumerable<PeopleUser> Get()
        {
            try
            {
                string userId = Request.Headers["userId"];
                List<PeopleUser> data = _context.PeopleUsers.Where(el => el.UserId == userId).ToList();
                return data;
            }
            catch (Exception)
            {
                return new List<PeopleUser>();
            }
        }

        [HttpGet("{id}")]
        [Authorized]
        public PeopleUser Get(string id)
        {
            try
            {
                string userId = Request.Headers["userId"];
                return _context.PeopleUsers.FirstOrDefault(val => val.Id == id && val.UserId == userId) ?? new PeopleUser();
            }
            catch (Exception)
            {
                return new PeopleUser();
            }
        }

        [HttpPost]
        [Authorized]
        public void Post([FromBody]PeopleUser value)
        {
            try
            {
                string userId = Request.Headers["userId"];
                value.UserId = userId;
                _context.PeopleUsers.Add(value);
                _context.SaveChanges();
                _hubContext.Clients.All.SendAsync("AddUser", value.Id);
            }
            catch (Exception) { }
        }
    }
}
