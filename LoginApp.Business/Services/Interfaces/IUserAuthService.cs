using LoginApp.Business.DTOs.login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginApp.Business.Services.Interfaces
{
    public interface IUserAuthService
    {
        Task<bool> Register(RegisterDTO request);
        Task<(bool Success, string? AccessToken, string? RefreshToken, string? Role)> Login(LoginDTO request);
        Task<(string AccessToken, string RefreshToken)> Refresh(string refreshTokenValue);
        Task CancelDeviceToken(int userId, string deviceId);
    }
}
