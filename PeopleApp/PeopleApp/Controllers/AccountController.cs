using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using PeopleApp.DatabaseContext;
using PeopleApp.Models;
using PeopleApp.Users;

namespace PeopleApp.Controllers
{
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private readonly Context _context;

        public AccountController(Context context)
        {
            _context = context;
        }

        [HttpPost]
        public Guid Post([FromBody]User user)
        {
            User u = _context.Users.FirstOrDefault(acc => acc.Login == user.Login && acc.Password == user.Password);
            if (u == null)
            {
                return Guid.Empty;
            }
            Guid guid = Guid.NewGuid();
            ListUsers.GuidList.AddGuid(guid);
            return guid;
        }
    }
}
