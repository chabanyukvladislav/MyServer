using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace PeopleApp.Controllers
{
    [Route("api/[controller]")]
    public class AuthenticationController : Controller
    {
        [HttpPost]
        public string PostLogin(string login, string password)
        {
            return "value";
        }
    }
}
