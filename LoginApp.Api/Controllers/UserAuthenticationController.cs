using LoginApp.Business.DTOs.login;
using LoginApp.Business.Services;
using LoginApp.Business.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LoginApp.Api.Controllers
{

    [ApiController]
    [Route("api/auth")]
    public class UserAuthenticationController : ControllerBase
    {
        private readonly IUserService _authService;

        public UserAuthenticationController(IUserService AuthenticationService)
        {
            _authService = AuthenticationService;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterDTO request)
        {
            var success = _authService.Register(request);

            if (!success) return BadRequest(new { message = "Username already exists" });

            return Ok(new { message = "User registered successfully" });
        }


        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDTO request)
        {
            var result = _authService.Login(request);

            if (!result.Success)
                return Unauthorized(new { message = "Invalid username or password" });

            return Ok(new { message = "Login successful", token = result.Token, role = result.Role });
        }




    }
}


