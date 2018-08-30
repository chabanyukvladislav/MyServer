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
    [Route("api/Message")]
    public class MessageController : Controller
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly Context _context;

        public MessageController(Context context, IHubContext<NotificationHub> hub)
        {
            _context = context;
            _hubContext = hub;
        }

        [HttpGet]
        [Authorized]
        public IEnumerable<Message> Get()
        {
            try
            {
                string userId = Request.Headers["userId"];
                return _context.Messages.Where(el => el.From.UserId == userId || el.To.UserId == userId);
            }
            catch (Exception)
            {
                return new List<Message>();
            }
        }

        [HttpGet("{id}")]
        [Authorized]
        public Message Get(Guid id)
        {
            try
            {
                string userId = Request.Headers["userId"];
                return _context.Messages.FirstOrDefault(el => el.Id == id && (el.From.UserId == userId || el.To.UserId == userId));
            }
            catch (Exception)
            {
                return new Message();
            }
        }

        [HttpPost]
        [Authorized]
        public void Post([FromBody]Message value)
        {
            try
            {
                string userId = Request.Headers["userId"];
                value.From = _context.Users.Find(userId);
                _context.Messages.Add(value);
                _context.SaveChanges();
                _hubContext.Clients.All.SendAsync("AddMessage", value.Id);
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
                string userId = Request.Headers["userId"];
                Message message = _context.Messages.FirstOrDefault(el => el.Id == id);
                if (message == null)
                    return;
                _context.Messages.Remove(message);
                _context.SaveChanges();
                _hubContext.Clients.All.SendAsync("DeleteMessage", id);
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
