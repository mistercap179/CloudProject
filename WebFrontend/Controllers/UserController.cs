using Common;
using Common.FrontendModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace WebFrontend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IApiGateway proxy;
        public UserController(IApiGateway _proxy)
        {
            proxy = _proxy;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login()
        {
            using (var reader = new StreamReader(Request.Body))
            {
                var body = await reader.ReadToEndAsync();
                var jsonDocument = JsonDocument.Parse(body);

                if (jsonDocument.RootElement.TryGetProperty("email", out var emailElement) &&
                    jsonDocument.RootElement.TryGetProperty("password", out var passwordElement))
                {
                    string email = emailElement.GetString();
                    string password = passwordElement.GetString();

                    var user = await proxy.Login(email, password);

                    if (user == null)
                        return BadRequest(new { message = "Invalid email or password" });


                    return Ok(new { User = user });
                }
                else
                {
                    return BadRequest(new { message = "Invalid JSON structure" });
                }
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserModel newUser)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await proxy.Register(newUser);

                    if (user == null)
                    {
                        return BadRequest(new { message = "Invalid email or password" });
                    }

                    return Ok(new { User = user });
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

        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] UserModel updateUser)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var updatedUser = await proxy.Update(updateUser);

                    if (updatedUser == null)
                    {
                        return BadRequest(new { message = "Failed to update user" });
                    }

                    return Ok(new { User = updatedUser });
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
