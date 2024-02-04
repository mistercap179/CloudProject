using Common;
using Common.FrontendModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebFrontend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IApiGateway proxy;
        public ProductController(IApiGateway _proxy)
        {
            proxy = _proxy;
        }

        [HttpGet("getProducts")]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                List<ProductModel> products = await proxy.GetProducts();
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal Server Error" });
            }
        }
    }
}
