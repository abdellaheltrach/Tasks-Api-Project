using LoginApp.Business.DTOs.login;
using LoginApp.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace LoginApp.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [EnableRateLimiting("AuthPolicy")]
    public class AuthController : ControllerBase
    {
        private readonly IUserAuthService _authService;
        private readonly IConfiguration _config;
        private const string RefreshCookieName = "refreshToken";
        private const string JwtSectionName = "Jwt";
        private const string RefreshTokenExpireDaysKey = "RefreshTokenExpireDays";

        public AuthController(IUserAuthService AuthenticationService, IConfiguration Config)
        {
            _authService = AuthenticationService;
            _config = Config;
        }

        private CookieOptions GetRefreshCookieOptions(int expireDays)
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(expireDays)
            };
        }

        [HttpGet("me")]
        [Authorize]
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
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _authService.Register(request);

            if (!success) return BadRequest(new { message = "Username already exists" });

            return Ok(new { message = "User registered successfully" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.Login(dto);
            if (!result.Success) //login refused
                return Unauthorized();

            var jwt = _config.GetSection(JwtSectionName);
            var expireDays = Convert.ToInt32(jwt[RefreshTokenExpireDaysKey]);

            // Set HttpOnly cookie for refresh token
            Response.Cookies.Append(RefreshCookieName, result.RefreshToken!, GetRefreshCookieOptions(expireDays));

            return Ok(new { accessToken = result.AccessToken });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return BadRequest(new { message = "Token is still valid" });
            }

            if (!Request.Cookies.TryGetValue(RefreshCookieName, out var cookieToken))
                return Unauthorized();

            try
            {
                var (newAccess, newRefresh) = await _authService.Refresh(cookieToken);
                var jwt = _config.GetSection(JwtSectionName);
                var expireDays = Convert.ToInt32(jwt[RefreshTokenExpireDaysKey]);

                Response.Cookies.Append(RefreshCookieName, newRefresh, GetRefreshCookieOptions(expireDays));

                return Ok(new { accessToken = newAccess });
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException || ex is ArgumentException)
            {
                Response.Cookies.Delete(RefreshCookieName);
                return Unauthorized();
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutDTO dto)
        {
            var jwt = _config.GetSection(JwtSectionName);
            var expireDays = Convert.ToInt32(jwt[RefreshTokenExpireDaysKey]);
            var cookieOptions = GetRefreshCookieOptions(expireDays);
            cookieOptions.Expires = null; // Remove expiration for deletion

            // Try to get user ID from claims
            if (!int.TryParse(User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
            {
                Response.Cookies.Delete(RefreshCookieName, cookieOptions);
                return Ok();
            }

            // Cancel token for the specific device if deviceId is provided
            if (!string.IsNullOrWhiteSpace(dto.DeviceId))
            {
                await _authService.CancelDeviceToken(userId, dto.DeviceId);
            }

            Response.Cookies.Delete(RefreshCookieName, cookieOptions);

            return Ok();
        }
    }
}



