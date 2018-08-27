using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XamarinClient.Models;

namespace XamarinClient.Services
{
    public interface IDataStore
    {
        bool IsConnect { get; }

        Task TryConnect();
        void Synchronized();
        Task<bool> AddItemAsync(People item);
        Task<bool> UpdateItemAsync(People item);
        Task<bool> DeleteItemAsync(Guid id);
        Task<List<People>> GetItemsAsync();
        Task<People> GetItemAsync(Guid id);

        event Action OnDisconnect;
    }
}
