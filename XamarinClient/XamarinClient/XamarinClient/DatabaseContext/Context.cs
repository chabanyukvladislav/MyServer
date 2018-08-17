using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;
using Xamarin.Forms;
using XamarinClient.Models;

namespace XamarinClient.DatabaseContext
{
    internal sealed class Context : DbContext
    {
        private const string DatabaseName = "database.db";

        public Context()
        {
            Database.EnsureCreated();
            Peoples.CountAsync();
        }

        public DbSet<People> Peoples { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connection;
            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                    connection = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), DatabaseName);
                    break;
                case Device.UWP:
                    connection = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DatabaseName);
                    break;
                case Device.iOS:
                    Batteries.Init();
                    connection = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "Library", DatabaseName);
                    break;
                default:
                    throw new NotImplementedException("Platform not supported");
            }
            optionsBuilder.UseSqlite($"Filename={connection}");
        }
    }
}
