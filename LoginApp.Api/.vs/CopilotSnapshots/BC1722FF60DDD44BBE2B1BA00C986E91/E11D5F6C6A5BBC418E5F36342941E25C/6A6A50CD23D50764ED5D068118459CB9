using LoginApp.Business.Constants;
using LoginApp.Business.DTOs.login;
using LoginApp.Business.Helpers;
using LoginApp.Business.Services.Interfaces;
using LoginApp.DataAccess.Entities;
using LoginApp.DataAccess.Repositories;
using LoginApp.DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

public class UserAuthService : IUserAuthService
{
    private readonly IUserRepository _UserRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly ITokenService _Token;

    public UserAuthService(IUserRepository userRepository, ITokenService Token, IRefreshTokenRepository refreshTokenRepo)
    {
        _UserRepository = userRepository;
        _refreshTokenRepo = refreshTokenRepo;
        _Token = Token;
    }

    public async Task<bool> Register(RegisterDTO requestDto)
    {
        if (await _UserRepository.FindUserByUserNameAsync(requestDto.Username) != null) //User Name already taken
        {
            return false;
        }

        var NewUser = new User
        {
            Role = UserRoles.Guest,
            Username = requestDto.Username,
            PasswordHash = clsPasswordHasher.Hash(requestDto.Password)
        };

        await _UserRepository.AddUserAsync(NewUser);
        await _UserRepository.SaveChangesAsync();
        return true;

    }

    public async Task<(bool Success, string? AccessToken, string? RefreshToken, string? Role)> Login(LoginDTO requestDto)
    {
        var user = await _UserRepository.FindUserByUserNameAsync(requestDto.Username);

        if (user == null)    //No UserName match
            return (false, null, null, null);

        if (!clsPasswordHasher.Verify(requestDto.Password, user.PasswordHash)) //Password incorrect
            return (false, null, null, null);

        //verify token 

        var deviceId = string.IsNullOrWhiteSpace(requestDto.deviceId) ? Guid.NewGuid().ToString() : requestDto.deviceId;
        var deviceName = string.IsNullOrWhiteSpace(requestDto.deviceName) ? "Unknown Device" : requestDto.deviceName;


        // Generate tokens
        var accessToken = _Token.GenerateAccessToken(user.Id, user.Username, user.Role ?? UserRoles.Guest);
        var refreshToken = _Token.GenerateRefreshToken(deviceId, deviceName);

        refreshToken.UserId = user.Id;


        // Update existingToken token for the same device
        var existingToken = await _refreshTokenRepo.GetByUserAndDeviceAsync(user.Id, deviceId);

        if (existingToken != null)
        {
            existingToken.Token = refreshToken.Token;
            existingToken.ExpiresDate = refreshToken.ExpiresDate;//update Token 


            await _refreshTokenRepo.UpdateAsync(existingToken);
            await _refreshTokenRepo.SaveChangesAsync();
            return (true, accessToken, refreshToken.Token, user.Role);
        }


        // Save new refresh token
        await _refreshTokenRepo.AddAsync(refreshToken);
        await _refreshTokenRepo.SaveChangesAsync();

        return (true, accessToken, refreshToken.Token, user.Role);
    }


    public async Task<(string AccessToken, string RefreshToken)> Refresh(string refreshTokenValue)
    {
        //  Get the token from DB
        var storedToken = await _refreshTokenRepo.GetTokenByTokenStringAsync(refreshTokenValue);

        if (storedToken == null || !storedToken.IsActive)
            throw new UnauthorizedAccessException("Invalid refresh token.");

        //  Cancel the current token (used token cannot be reused)
        storedToken.IsCanceled = true;
        await _refreshTokenRepo.UpdateAsync(storedToken);

        //  Generate a new refresh token
        var newRefresh = _Token.GenerateRefreshToken(storedToken.DeviceId, storedToken.DeviceName);
        newRefresh.UserId = storedToken.UserId;

        // 4️⃣ Instead of adding a new token, override the old token record
        storedToken.Token = newRefresh.Token;
        storedToken.ExpiresDate = newRefresh.ExpiresDate;
        storedToken.IsCanceled = false;

        await _refreshTokenRepo.UpdateAsync(storedToken);
        await _refreshTokenRepo.SaveChangesAsync();

        //  Generate new access token
        var newAccess = _Token.GenerateAccessToken(storedToken.User.Id, storedToken.User.Username, storedToken.User.Role ?? UserRoles.Guest);

        return (newAccess, storedToken.Token);
    }

    public async Task CancelDeviceToken(int userId, string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            return;

        // Get the refresh token for this user and device
        var token = await _refreshTokenRepo.GetByUserAndDeviceAsync(userId, deviceId);

        if (token != null && token.IsActive)
        {
            token.IsCanceled = true;
            await _refreshTokenRepo.UpdateAsync(token);
            await _refreshTokenRepo.SaveChangesAsync();
        }
    }


}
