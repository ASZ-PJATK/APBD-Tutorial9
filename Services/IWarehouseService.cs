using Tutorial9.Model.DTO_s;

namespace Tutorial9.Services;

public interface IWarehouseService
{
    Task<List<WarehouseDTO>> GetWarehouses();
    Task<int> PostWPO(WarehouseDTO warehouseDto, int IdOrder);
    Task<bool> WarehouseExists(int id);
    Task<bool> OrderFullfilled(int idOrder);
    Task<bool> InsertVIAProcedure(WarehouseDTO warehouseDto);
}