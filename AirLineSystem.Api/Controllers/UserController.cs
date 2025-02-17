using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AirLineSystem.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public UserController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        [HttpPost("CheckUserExistsAndMakeAdmin")]
        [Authorize]
        public IActionResult CheckUserExistsAndMakeAdmin([FromBody] UserRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new { Message = "Invalid user email." });
            }

         
            string adminEmail = _configuration["UserEmail"]; 
            if (adminEmail.Equals(request.Email, StringComparison.OrdinalIgnoreCase))
            {
               
                return Ok(new { isAdmin = true, Message = "User is an Admin." });
            }

        
            return Ok(new { isAdmin = false, Message = "User is not an Admin." });
        }

        public class UserRequest
        {
            public string Email { get; set; }
        }
    }
}
