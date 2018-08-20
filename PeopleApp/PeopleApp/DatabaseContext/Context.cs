using Microsoft.EntityFrameworkCore;
using PeopleApp.Models;

namespace PeopleApp.DatabaseContext
{
    public sealed class Context : DbContext
    {
        public DbSet<People> Peoples { get; set; }

        public Context(DbContextOptions<Context> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}