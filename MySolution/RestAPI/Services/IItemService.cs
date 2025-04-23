using RestAPI.Dtos;

namespace RestAPI.Services;

public interface IItemService
{
    IEnumerable<ItemResponse> GetAll();
    ItemResponse? GetById(Guid id);
    ItemResponse Create(ItemRequest request);
    bool Update(Guid id, ItemRequest request);
    bool Delete(Guid id);
}
