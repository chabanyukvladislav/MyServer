using System.Collections.Generic;
using System.Collections.Specialized;

namespace XamarinClient.ViewModels
{
    internal class UsersCollection : INotifyCollectionChanged
    {
        private static readonly object Locker = new object();
        private static UsersCollection _usersCollection;

        public static UsersCollection GetUsersCollection
        {
            get
            {
                if (_usersCollection == null)
                {
                    lock (Locker)
                    {
                        if (_usersCollection == null)
                        {
                            _usersCollection = new UsersCollection();
                        }
                    }
                }

                return _usersCollection;
            }
        }
        private List<string> Users { get; set; }

        private UsersCollection()
        {
            Users = new List<string>();
        }

        public void Add(string userId)
        {
            Users.Add(userId);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, userId);
        }

        public List<string> GetCollection()
        {
            return Users;
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        protected void OnCollectionChanged(NotifyCollectionChangedAction action, string item)
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, item));
        }
    }
}