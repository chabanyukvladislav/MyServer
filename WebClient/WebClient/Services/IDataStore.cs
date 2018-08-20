using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebClient.Models;

namespace WebClient.Services
{
    public interface IDataStore
    {
        Task<bool> AddItemAsync(People item);
        Task<bool> UpdateItemAsync(People item);
        Task<bool> DeleteItemAsync(Guid id);
        Task<List<People>> GetItemsAsync();
        Task<People> GetItemAsync(Guid id);
    }
}
