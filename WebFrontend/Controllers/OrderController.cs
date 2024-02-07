using Common;
using Common.FrontendModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebFrontend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IApiGateway proxy;
        public OrderController(IApiGateway _proxy)
        {
            proxy = _proxy;
        }

        [HttpGet("getOrders/{userId}")]
        public async Task<IActionResult> GetOrders(string userId)
        {
            try
            {
                List<OrderModel> orders = await proxy.GetOrders(userId);

                if (orders == null || orders.Count == 0)
                {
                    return NotFound(new { message = "No orders found for the specified user." });
                }

                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal Server Error" });
            }
        }

        [HttpPost("createOrder")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderModel order)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await proxy.CreateOrder(order);

                    if (result == "Transaction successful")
                    {
                        return Ok(new { message = "Order created successfully" });
                    }
                    else
                    {
                        return BadRequest(new { message = "Order creation failed" });
                    }
                }
                else
                {
                    return BadRequest(new { message = "Invalid model state" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal Server Error" });
            }
        }
    }
}
