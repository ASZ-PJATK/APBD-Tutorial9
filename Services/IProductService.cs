namespace Tutorial9.Services;

public interface IProductService
{
    Task<bool> ProductExists(int id);
    Task<decimal> GetProductPrice(int warehouseDtoIdProduct);
}