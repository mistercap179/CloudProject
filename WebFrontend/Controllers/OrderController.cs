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
    }
}
