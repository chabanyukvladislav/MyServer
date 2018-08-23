using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebClient.Models;

namespace WebClient.Services
{
    public interface IDataStore
    {
        Task<bool> AddItemAsync(People item, string userId);
        Task<bool> UpdateItemAsync(People item, string userId);
        Task<bool> DeleteItemAsync(Guid id, string userId);
        Task<List<People>> GetItemsAsync(string userId);
        Task<People> GetItemAsync(Guid id, string userId);
    }
}
