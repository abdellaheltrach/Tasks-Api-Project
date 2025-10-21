using LoginApp.Api.DTOs;
using LoginApp.Business;
using Microsoft.AspNetCore.Mvc;

namespace LoginApp.Api.Controllers
{

    [ApiController]
    [Route("api/login")]
    public class UserAuthenticationController : ControllerBase
    {
        private readonly UserService _authService;

        public UserAuthenticationController(UserService AuthenticationService)
        {
            _authService = AuthenticationService;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            var success = _authService.Register(request.Username, request.Password);

            if (!success) return BadRequest(new { message = "Username already exists" });

            return Ok(new { message = "User registered successfully" });
        }


        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var success = _authService.Login(request.Username, request.Password, out string role);

            if (!success) return Unauthorized(new { message = "Invalid username or password" });

            return Ok(new { message = "Login successful", role });
        }
    }
}


