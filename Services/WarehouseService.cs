using System.Data;
using Microsoft.Data.SqlClient;
using Tutorial9.Model.DTO_s;

namespace Tutorial9.Services;

public class WarehouseService : IWarehouseService
{
    private readonly IConfiguration _configuration;
    private readonly IProductService _productService;

    public WarehouseService(IConfiguration configuration, IProductService productService)
    {
        _configuration = configuration;
        _productService = productService;
    }
    
    public async Task<bool> InsertVIAProcedure(WarehouseDTO warehouseDto)
    {
        try
        {
            await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
            await using SqlCommand command = new SqlCommand();

            command.Connection = connection;
            await connection.OpenAsync();

            command.CommandText = "AddProductToWarehouse";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@IdProduct", warehouseDto.IdProduct);
            command.Parameters.AddWithValue("@IdWarehouse", warehouseDto.IdWarehouse);
            command.Parameters.AddWithValue("@Amount", warehouseDto.Amount);
            command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);


            var count = await command.ExecuteNonQueryAsync();
            if (count == 0)
            {
                return false;
            }

            return true;
        }
        catch (SqlException e)
        {
            throw new Exception($"{e}",e);
        }
        
    }

    public async Task<List<WarehouseDTO>> GetWarehouses()
    {
        var WPO = new List<WarehouseDTO>();

        string command = "Select IdWarehouse,IdProduct,Amount,CreatedAt from Product_Warehouse";

        using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("Default")))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            await conn.OpenAsync();
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    WPO.Add(new WarehouseDTO()
                    {
                        IdWarehouse = reader.GetInt32(reader.GetOrdinal("IdWarehouse")),
                        Amount = reader.GetInt32(reader.GetOrdinal("Amount")),
                        IdProduct = reader.GetInt32(reader.GetOrdinal("IdProduct")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                    });
                }
            }
        }

        return WPO;
    }

    public async Task<int> PostWPO(WarehouseDTO warehouseDto, int IdOrder)
    {
        string command = @"Insert into Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
                            values (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt);
                            select scope_identity()";

        await using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("Default")))
        await using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("IdWarehouse", warehouseDto.IdWarehouse);
            cmd.Parameters.AddWithValue("IdProduct", warehouseDto.IdProduct);
            cmd.Parameters.AddWithValue("Price", await _productService.GetProductPrice(warehouseDto.IdProduct) * warehouseDto.Amount);
            cmd.Parameters.AddWithValue("IdOrder", IdOrder);
            cmd.Parameters.AddWithValue("Amount", warehouseDto.Amount);
            cmd.Parameters.AddWithValue("CreatedAt", DateTime.Now);

            conn.OpenAsync();
            
            object result = cmd.ExecuteScalar();
            return Convert.ToInt32(result);

        }
    }

    public async Task<bool> WarehouseExists(int id)
    {
        if (id <= 0)
        {
            return false;
        }
        var command = $@"Select 1 from Warehouse where IdWarehouse = @Id";
        
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

    public async Task<bool> OrderFullfilled(int idOrder)
    {
        var command = $@"Select 1 from Product_Warehouse where IdOrder = @Id";
        
        await using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("Default")))
        await using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("Id", idOrder);

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
}