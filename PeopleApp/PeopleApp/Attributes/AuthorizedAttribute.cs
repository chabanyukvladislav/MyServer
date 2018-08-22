using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using PeopleApp.DatabaseContext;

namespace PeopleApp.Attributes
{
    public class AuthorizedAttribute : TypeFilterAttribute
    {
        public AuthorizedAttribute() : base(typeof(AuthorizedFilter)) { }
    }

    public class AuthorizedFilter : IAuthorizationFilter
    {
        private readonly Context _context;

        public AuthorizedFilter(Context context)
        {
            _context = context;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue("UserId", out StringValues userId) || !_context.Users.Any(el => el.UserId == userId))
                context.Result = new UnauthorizedResult();
        }
    }
}
