using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestAPI.Models;
using RestAPI.Dtos;

namespace RestAPI.Services;

public class ItemService : IItemService
{
    // Thread-safe collections for concurrent access
    private readonly ConcurrentDictionary<Guid, Item> _items = new();

    public async Task<ItemResponse?> GetByIdAsync(Guid id)
    {
        return await Task.Run(() =>
        {
            if (!_items.TryGetValue(id, out var item))
                return null;

            return new ItemResponse
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description
            };
        });
    }
}
