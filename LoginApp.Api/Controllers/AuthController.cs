using LoginApp.Business.DTOs.login;
using LoginApp.Business.Services;
using LoginApp.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LoginApp.Api.Controllers
{

    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserAuthService _authService;
        private readonly IConfiguration _config;
        private const string RefreshCookieName = "refreshToken";

        public AuthController(IUserAuthService AuthenticationService, IConfiguration Config)
        {
            _authService = AuthenticationService;
            _config = Config;
        }

        // GET: api/auth/me
        [HttpGet("me")]
        [Authorize] // only accessible if JWT is valid
        public IActionResult Me()
        {
            // Extract info from JWT claims
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = User.Identity?.Name;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userId == null) return Unauthorized();

            return Ok(new
            {
                id = userId,
                username,
                role
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO request)
        {
            var success = await _authService.Register(request);

            if (!success) return BadRequest(new { message = "Username already exists" });

            return Ok(new { message = "User registered successfully" });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            var result = await _authService.Login(dto);
            if (!result.Success) //login refused
                return Unauthorized();


            var jwt = _config.GetSection("Jwt");

            // Get expiration days from appsettings
            var expireDays = Convert.ToInt32(jwt["RefreshTokenExpireDays"]);

           
            // Set HttpOnly cookie for refresh token
            Response.Cookies.Append(RefreshCookieName, result.RefreshToken!, new CookieOptions
            {
                HttpOnly = true,                 // JS on client cannot read this cookie.
                Secure = true,                   // Only send cookie over HTTPS.
                SameSite = SameSiteMode.None,  // SameSiteMode.Strict: Only send cookie for same-site requests.
                Expires = DateTime.UtcNow.AddDays(expireDays)// Cookie automatically expires after jwt["RefreshTokenExpireDays"]
            });

            return Ok(new { accessToken = result.AccessToken, role = result.Role });
        }


        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            if (!Request.Cookies.TryGetValue(RefreshCookieName, out var cookieToken))
                return Unauthorized();
            
            try
            {
                var (newAccess, newRefresh) = await _authService.Refresh(cookieToken);


                // Get expiration days from appsettings
                var jwt = _config.GetSection("Jwt");
                var expireDays = Convert.ToInt32(jwt["RefreshTokenExpireDays"]);


                Response.Cookies.Append(RefreshCookieName, newRefresh, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddDays(expireDays)
                });

                return Ok(new { accessToken = newAccess });
            }
            catch
            {
                Response.Cookies.Delete(RefreshCookieName);
                return Unauthorized();
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutDTO dto)
        {
            // Define the options exactly as they were used during creation
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None
            };

            // Try to get user ID from claims
            if (!int.TryParse(User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
            {
                // Delete refresh token cookie with matching options
                Response.Cookies.Delete(RefreshCookieName, cookieOptions);
                return Ok();
            }

            // Cancel token for the specific device if deviceId is provided
            if (!string.IsNullOrWhiteSpace(dto.DeviceId))
            {
                await _authService.CancelDeviceToken(userId, dto.DeviceId);
            }

            // Delete refresh token cookie with matching options
            Response.Cookies.Delete(RefreshCookieName, cookieOptions);

            return Ok();
        }
    }


}



