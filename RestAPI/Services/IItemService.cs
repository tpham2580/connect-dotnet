using System.Collections.Generic;
using System.Threading.Tasks;
using RestAPI.Dtos;

namespace RestAPI.Services;

public interface IItemService
{
    // IEnumerable<ItemResponse> GetAll();
    Task<ItemResponse?> GetByIdAsync(Guid id);
    // ItemResponse Create(ItemRequest request);
    // bool Update(Guid id, ItemRequest request);
    // bool Delete(Guid id);
}
