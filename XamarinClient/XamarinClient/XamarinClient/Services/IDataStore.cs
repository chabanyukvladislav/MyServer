using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XamarinClient.Enum;
using XamarinClient.Models;

namespace XamarinClient.Services
{
    public interface IDataStore
    {
        bool IsConnect { get; }

        Task<Result> Synchronized();
        Task<Result> AddItemAsync(People item);
        Task<Result> UpdateItemAsync(People item);
        Task<Result> DeleteItemAsync(Guid id);
        Task<List<People>> GetItemsAsync();
        Task<People> GetItemAsync(Guid id);
    }
}
