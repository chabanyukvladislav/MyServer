using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using XamarinClient.Collections;
using XamarinClient.DatabaseContext;
using XamarinClient.Models;

namespace XamarinClient.Services
{
    class DbDataStore : IDataStore
    {
        private static readonly object Locker = new object();
        private static DbDataStore _dataStore;
        private readonly Context _context;

        public static DbDataStore GetDataStore
        {
            get
            {
                if (_dataStore == null)
                {
                    lock (Locker)
                    {
                        if (_dataStore == null)
                        {
                            _dataStore = new DbDataStore();
                        }
                    }
                }
                return _dataStore;
            }
        }

        private DbDataStore()
        {
            _context = new Context();
        }

        public bool IsConnect => true;

        public async Task TryConnect()
        {
            await Task.Run(() =>
            {
                IDataStore ds = ServerDataStore.GetDataStore;
                while (!ds.IsConnect)
                {
                    Thread.Sleep(1000);
                }

                OnDisconnect?.Invoke();
            });
        }

        public void Synchronized()
        {
            try
            {
                IEnumerable<People> list = PhonesCollection.GetPhonesCollection.GetCollection() ?? new List<People>();
                if (!list.Any())
                    return;
                _context.Peoples.RemoveRange(_context.Peoples);
                _context.Peoples.AddRange(list);
                _context.SaveChanges();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public async Task<bool> AddItemAsync(People item)
        {
            return await Task.Run(() =>
            {
                _context.Peoples.Add(item);
                Synchronizer.AddItem(item);
                return true;
            });
        }

        public async Task<bool> UpdateItemAsync(People item)
        {
            return await Task.Run(() =>
            {
                People val = _context.Peoples.FirstOrDefault(el => el.Id == item.Id);
                if (val == null)
                    return false;
                val.Name = item.Name;
                val.Phone = item.Phone;
                val.Surname = item.Surname;
                Synchronizer.EditItem(item);
                return true;
            });
        }

        public async Task<bool> DeleteItemAsync(Guid id)
        {
            return await Task.Run(() =>
            {
                try
                {
                    People val = _context.Peoples.FirstOrDefault(el => el.Id == id);
                    if (val == null)
                        return false;
                    _context.Peoples.Remove(val);
                    _context.SaveChanges();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            });
        }

        public async Task<List<People>> GetItemsAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    return _context.Peoples.ToList();
                }
                catch (Exception)
                {
                    return new List<People>();
                }
            });
        }

        public async Task<People> GetItemAsync(Guid id)
        {
            return await Task.Run(() =>
            {
                try
                {
                    People val = _context.Peoples.FirstOrDefault(el => el.Id == id);
                    if (val == null)
                        return new People();
                    return val;
                }
                catch (Exception)
                {
                    return new People();
                }
            });
        }

        public async Task<People> GetItemAsync(People value)
        {
            return await Task.Run(() =>
            {
                try
                {
                    People val = _context.Peoples.FirstOrDefault(el => el.Name == value.Name && el.Surname == value.Surname && el.Phone == value.Phone);
                    if (val == null)
                        return new People();
                    return val;
                }
                catch (Exception)
                {
                    return new People();
                }
            });
        }


        public event Action OnDisconnect;
    }
}
