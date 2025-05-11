namespace Tutorial9.Services;

public interface IOrderService
{
    Task<int> OrderForProductEligible(int warehouseDtoAmount, int warehouseDtoIdProduct, DateTime warehouseDtoCreatedAt);
    void FullfillOrder(int orderid);
}