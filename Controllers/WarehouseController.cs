using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Tutorial9.Model.DTO_s;
using Tutorial9.Services;

namespace Tutorial9.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehouseController: ControllerBase
    {
        private readonly IWarehouseService _warehouseService;
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;

        public WarehouseController(IWarehouseService warehouseService, IProductService productService, IOrderService orderService)
        {
            _warehouseService = warehouseService;
            _productService = productService;
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetWarehouses()
        {
            var warehouses = await _warehouseService.GetWarehouses();
            return Ok(warehouses);
        }
        
        [HttpPost("procedure")]
        public async Task<IActionResult> CreateWPOproc([FromBody] WarehouseDTO dto)
        {
            try
            {
                if (await _warehouseService.InsertVIAProcedure(dto))
                {
                    return Created($"{dto.IdProduct}", dto);
                }

                return Conflict();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> CreateWPO([FromBody] WarehouseDTO dto)
        {
            if (!await _productService.ProductExists(dto.IdProduct))
            {
                return NotFound("This product does not exist :(");
            }
            else if (!await _warehouseService.WarehouseExists(dto.IdWarehouse))
            {
                return NotFound("This warehouse does not exist :(");
            }

            var orderid = await _orderService.OrderForProductEligible(dto.Amount, dto.IdProduct,
                dto.CreatedAt);
            if (orderid == -1)
            {
                return NotFound("No eligible order for this product :(");
            }
            else if (await _warehouseService.OrderFullfilled(orderid))
            {
                return Conflict("This order has already been fulfilled.");
            }
            
            _orderService.FullfillOrder(orderid);
            
            var pk = await _warehouseService.PostWPO(dto,orderid);
            return Ok(pk);
        }
    } 
}

