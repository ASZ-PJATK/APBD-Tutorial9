using Microsoft.Data.SqlClient;

namespace Tutorial9.Services;

public class OrderService : IOrderService
{
    private readonly IConfiguration _configuration;

    public OrderService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public async Task<int> OrderForProductEligible(int warehouseDtoAmount, int warehouseDtoIdProduct, DateTime warehouseDtoCreatedAt)
    {
        var command = $@"Select * from [Order] where IdOrder = @IdOrder and IdProduct = @IdProduct";
        
        await using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("Default")))
        await using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("IdOrder", warehouseDtoIdProduct);
            cmd.Parameters.AddWithValue("IdProduct", warehouseDtoIdProduct);
            
            await conn.OpenAsync();

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    if (warehouseDtoCreatedAt > reader.GetDateTime(reader.GetOrdinal("CreatedAt")))
                    {
                        return reader.GetInt32(reader.GetOrdinal("IdOrder"));
                    }
                }
            }
        }

        return -1;
    }

    public async void FullfillOrder(int orderid)
    {
        var now = DateTime.Now;
        var command = $@"Update Order set FulfilledAt = @Now where IdOrder = @Id";
        
        await using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("Default")))
        await using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("Id", orderid);
            cmd.Parameters.AddWithValue("Now", now);

            await conn.OpenAsync();

            //int rowsAffected = cmd.ExecuteNonQuery();
        }
    }
}