using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PeopleApp.Users
{
    public class ListUsers
    {
        private static ListUsers _listUsers;
        private static readonly object Locker = new object();

        public static ListUsers GuidList
        {
            get
            {
                if (_listUsers == null)
                {
                    lock (Locker)
                    {
                        if (_listUsers == null)
                            _listUsers = new ListUsers();
                    }
                }

                return _listUsers;
            }
        }

        private Dictionary<Guid, DateTime> List { get; }

        private ListUsers()
        {
            List = new Dictionary<Guid, DateTime>();
            RemoveGuid();
        }

        public void AddGuid(Guid guid)
        {
            List.Add(guid,DateTime.Now);
        }

        public bool IsLogin(Guid guid)
        {
            return List.ContainsKey(guid);
        }

        private async void RemoveGuid()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000 * 60 * 60 * 24);
                    List<Guid> list = List.Keys.Where(x => List[x].Date.Day + 7 < DateTime.Now.Day).ToList();
                    foreach (Guid guid in list)
                    {
                        List.Remove(guid);
                    }
                }
            });
        }
    }
}
