using Microsoft.Data.SqlClient;

namespace Tutorial9.Services;

public class ProductService : IProductService
{
    private readonly IConfiguration _configuration;

    public ProductService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public async Task<bool> ProductExists(int id)
    {
        if (id <= 0)
        {
            return false;
        }
        var command = $@"Select 1 from Product where IdProduct = @Id";
        
        await using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("Default")))
        await using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("Id", id);

            await conn.OpenAsync();

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    return true;
                }
            }
        }

        return false;
    }

    public async Task<decimal> GetProductPrice(int warehouseDtoIdProduct)
    {
        string command = @$"SELECT * FROM Product where IdProduct = @Id";
        
        using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("Default")))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("Id", warehouseDtoIdProduct);
            await conn.OpenAsync();

            Console.WriteLine(cmd.CommandText.ToString());
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    var price = reader.GetDecimal(reader.GetOrdinal("Price"));
                    return price;
                }
                else
                {
                    throw new Exception("No product with this id ");
                }
            }
        }
    }
}