using FluentAssertions;
using LoginApp.Business.Constants;
using LoginApp.Business.DTOs;
using LoginApp.Business.DTOs.login;
using LoginApp.Business.Helpers;
using LoginApp.Business.Services;
using LoginApp.Business.Services.Interfaces;
using LoginApp.DataAccess.Entities;
using LoginApp.DataAccess.Repositories.Interfaces;
using LoginAppTest.Helpers;
using Moq;

namespace LoginAppTest.Unit.Services;

/// <summary>
/// Unit tests for UserAuthService covering registration, login, token refresh, and logout operations.
/// Tests critical security flows including password hashing and refresh token management.
/// </summary>
public class UserAuthServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<IRefreshTokenRepository> _mockTokenRepo;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly UserAuthService _service;

    public UserAuthServiceTests()
    {
        _mockUserRepo = new Mock<IUserRepository>();
        _mockTokenRepo = new Mock<IRefreshTokenRepository>();
        _mockTokenService = new Mock<ITokenService>();
        _service = new UserAuthService(_mockUserRepo.Object, _mockTokenService.Object,_mockTokenRepo.Object);
    }

    #region Register Tests

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "UserAuthService")]
    public async Task Register_WithValidData_ReturnsTrue()
    {
        // Arrange
        var registerDto = new RegisterDTO
        {
            Username = "newuser",
            Password = "SecurePass123"
        };

        _mockUserRepo.Setup(r => r.FindUserByUserNameAsync(registerDto.Username))
                     .ReturnsAsync((User?)null); // User doesn't exist
        _mockUserRepo.Setup(r => r.AddUserAsync(It.IsAny<User>()))
                     .Returns(Task.CompletedTask);
        _mockUserRepo.Setup(r => r.SaveChangesAsync())
                     .Returns(Task.CompletedTask);

        // Act
        var result = await _service.Register(registerDto);

        // Assert
        result.Should().BeTrue();
        _mockUserRepo.Verify(r => r.AddUserAsync(It.Is<User>(u =>
            u.Username == registerDto.Username &&
            u.Role == UserRoles.Guest
        )), Times.Once);
        _mockUserRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "UserAuthService")]
    public async Task Register_WithDuplicateUsername_ReturnsFalse()
    {
        // Arrange
        var registerDto = new RegisterDTO
        {
            Username = "existinguser",
            Password = "SecurePass123"
        };

        var existingUser = new User
        {
            Id = 1,
            Username = "existinguser"
        };

        _mockUserRepo.Setup(r => r.FindUserByUserNameAsync(registerDto.Username))
                     .ReturnsAsync(existingUser); // User exists

        // Act
        var result = await _service.Register(registerDto);

        // Assert
        result.Should().BeFalse();
        _mockUserRepo.Verify(r => r.AddUserAsync(It.IsAny<User>()), Times.Never);
        _mockUserRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "UserAuthService")]
    public async Task Register_HashesPassword_Correctly()
    {
        // Arrange
        var registerDto = new RegisterDTO
        {
            Username = "testuser",
            Password = "PlainTextPassword"
        };

        User? capturedUser = null;
        _mockUserRepo.Setup(r => r.FindUserByUserNameAsync(It.IsAny<string>()))
                     .ReturnsAsync((User?)null);
        _mockUserRepo.Setup(r => r.AddUserAsync(It.IsAny<User>()))
                     .Callback<User>(u => capturedUser = u)
                     .Returns(Task.CompletedTask);
        _mockUserRepo.Setup(r => r.SaveChangesAsync())
                     .Returns(Task.CompletedTask);

        // Act
        await _service.Register(registerDto);

        // Assert
        capturedUser.Should().NotBeNull();
        capturedUser!.PasswordHash.Should().NotBe(registerDto.Password); // Password must be hashed
        clsPasswordHasher.Verify(registerDto.Password, capturedUser.PasswordHash).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "UserAuthService")]
    public async Task Register_SetsDefaultGuestRole()
    {
        // Arrange
        var registerDto = new RegisterDTO
        {
            Username = "roleTestUser",
            Password = "Password123"
        };

        User? capturedUser = null;
        _mockUserRepo.Setup(r => r.FindUserByUserNameAsync(It.IsAny<string>()))
                     .ReturnsAsync((User?)null);
        _mockUserRepo.Setup(r => r.AddUserAsync(It.IsAny<User>()))
                     .Callback<User>(u => capturedUser = u)
                     .Returns(Task.CompletedTask);
        _mockUserRepo.Setup(r => r.SaveChangesAsync())
                     .Returns(Task.CompletedTask);

        // Act
        await _service.Register(registerDto);

        // Assert
        capturedUser.Should().NotBeNull();
        capturedUser!.Role.Should().Be(UserRoles.Guest);
    }

    #endregion

    #region Login Tests

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "UserAuthService")]
    public async Task Login_WithValidCredentials_ReturnsSuccessWithTokens()
    {
        // Arrange
        var loginDto = new LoginDTO
        {
            Identifier = "validuser",
            Password = "ValidPassword123",
            deviceId = "device-001",
            deviceName = "My Laptop"
        };

        var user = new User
        {
            Id = 1,
            Username = "validuser",
            PasswordHash = clsPasswordHasher.Hash("ValidPassword123"),
            Role = UserRoles.Guest
        };

        var refreshToken = new RefreshToken
        {
            Token = "refresh-token-123",
            UserId = user.Id,
            DeviceId = loginDto.deviceId,
            DeviceName = loginDto.deviceName,
            ExpiresDate = DateTime.UtcNow.AddDays(7)
        };

        _mockUserRepo.Setup(r => r.FindUserByIdentifierAsync(loginDto.Identifier))
                     .ReturnsAsync(user);
        _mockTokenService.Setup(s => s.GenerateAccessToken(user.Id, user.Username, user.Role!))
                         .Returns("access-token-123");
        _mockTokenService.Setup(s => s.GenerateRefreshToken(loginDto.deviceId, loginDto.deviceName))
                         .Returns(refreshToken);
        _mockTokenRepo.Setup(r => r.GetByUserAndDeviceAsync(user.Id, loginDto.deviceId))
                      .ReturnsAsync((RefreshToken?)null); // No existing token
        _mockTokenRepo.Setup(r => r.AddAsync(It.IsAny<RefreshToken>()))
                      .Returns(Task.CompletedTask);
        _mockTokenRepo.Setup(r => r.SaveChangesAsync())
                      .Returns(Task.CompletedTask);

        // Act
        var result = await _service.Login(loginDto);

        // Assert
        result.Success.Should().BeTrue();
        result.AccessToken.Should().Be("access-token-123");
        result.RefreshToken.Should().Be("refresh-token-123");
        result.Role.Should().Be(UserRoles.Guest);
        _mockTokenRepo.Verify(r => r.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "UserAuthService")]
    public async Task Login_WithInvalidUsername_ReturnsFailure()
    {
        // Arrange
        var loginDto = new LoginDTO
        {
            Identifier = "nonexistent",
            Password = "Password123"
        };

        _mockUserRepo.Setup(r => r.FindUserByIdentifierAsync(loginDto.Identifier))
                     .ReturnsAsync((User?)null);

        // Act
        var result = await _service.Login(loginDto);

        // Assert
        result.Success.Should().BeFalse();
        result.AccessToken.Should().BeNull();
        result.RefreshToken.Should().BeNull();
        result.Role.Should().BeNull();
        _mockTokenService.Verify(s => s.GenerateAccessToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "UserAuthService")]
    public async Task Login_WithInvalidPassword_ReturnsFailure()
    {
        // Arrange
        var loginDto = new LoginDTO
        {
            Identifier = "validuser",
            Password = "WrongPassword"
        };

        var user = new User
        {
            Id = 1,
            Username = "validuser",
            PasswordHash = clsPasswordHasher.Hash("CorrectPassword")
        };

        _mockUserRepo.Setup(r => r.FindUserByIdentifierAsync(loginDto.Identifier))
                     .ReturnsAsync(user);

        // Act
        var result = await _service.Login(loginDto);

        // Assert
        result.Success.Should().BeFalse();
        result.AccessToken.Should().BeNull();
        _mockTokenService.Verify(s => s.GenerateAccessToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "UserAuthService")]
    public async Task Login_GeneratesDeviceId_WhenNotProvided()
    {
        // Arrange
        var loginDto = new LoginDTO
        {
            Identifier = "validuser",
            Password = "ValidPassword123",
            deviceId = "", // Empty device ID
            deviceName = ""
        };

        var user = new User
        {
            Id = 1,
            Username = "validuser",
            PasswordHash = clsPasswordHasher.Hash("ValidPassword123"),
            Role = UserRoles.Guest
        };

        string capturedDeviceId = "";
        _mockUserRepo.Setup(r => r.FindUserByIdentifierAsync(loginDto.Identifier))
                     .ReturnsAsync(user);
        _mockTokenService.Setup(s => s.GenerateAccessToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                         .Returns("access-token");
        _mockTokenService.Setup(s => s.GenerateRefreshToken(It.IsAny<string>(), It.IsAny<string>()))
                         .Callback<string, string>((deviceId, deviceName) => capturedDeviceId = deviceId)
                         .Returns(new RefreshToken { Token = "refresh", UserId = user.Id });
        _mockTokenRepo.Setup(r => r.GetByUserAndDeviceAsync(It.IsAny<int>(), It.IsAny<string>()))
                      .ReturnsAsync((RefreshToken?)null);
        _mockTokenRepo.Setup(r => r.AddAsync(It.IsAny<RefreshToken>()))
                      .Returns(Task.CompletedTask);
        _mockTokenRepo.Setup(r => r.SaveChangesAsync())
                      .Returns(Task.CompletedTask);

        // Act
        await _service.Login(loginDto);

        // Assert
        capturedDeviceId.Should().NotBeNullOrEmpty();
        Guid.TryParse(capturedDeviceId, out _).Should().BeTrue(); // Should be a GUID
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "UserAuthService")]
    public async Task Login_UpdatesExistingToken_ForSameDevice()
    {
        // Arrange
        var loginDto = new LoginDTO
        {
            Identifier = "validuser",
            Password = "ValidPassword123",
            deviceId = "device-001",
            deviceName = "My Laptop"
        };

        var user = new User
        {
            Id = 1,
            Username = "validuser",
            PasswordHash = clsPasswordHasher.Hash("ValidPassword123"),
            Role = UserRoles.Guest
        };

        var existingToken = new RefreshToken
        {
            Id = 1,
            Token = "old-refresh-token",
            UserId = user.Id,
            DeviceId = loginDto.deviceId,
            DeviceName = loginDto.deviceName,
            ExpiresDate = DateTime.UtcNow.AddDays(1)
        };

        var newRefreshToken = new RefreshToken
        {
            Token = "new-refresh-token",
            UserId = user.Id,
            DeviceId = loginDto.deviceId,
            DeviceName = loginDto.deviceName,
            ExpiresDate = DateTime.UtcNow.AddDays(7)
        };

        _mockUserRepo.Setup(r => r.FindUserByIdentifierAsync(loginDto.Identifier))
                     .ReturnsAsync(user);
        _mockTokenService.Setup(s => s.GenerateAccessToken(user.Id, user.Username, user.Role))
                         .Returns("access-token-123");
        _mockTokenService.Setup(s => s.GenerateRefreshToken(loginDto.deviceId, loginDto.deviceName))
                         .Returns(newRefreshToken);
        _mockTokenRepo.Setup(r => r.GetByUserAndDeviceAsync(user.Id, loginDto.deviceId))
                      .ReturnsAsync(existingToken); // Existing token found
        _mockTokenRepo.Setup(r => r.UpdateAsync(It.IsAny<RefreshToken>()))
                      .Returns(Task.CompletedTask);
        _mockTokenRepo.Setup(r => r.SaveChangesAsync())
                      .Returns(Task.CompletedTask);

        // Act
        var result = await _service.Login(loginDto);

        // Assert
        result.Success.Should().BeTrue();
        existingToken.Token.Should().Be("new-refresh-token"); // Token should be updated
        existingToken.ExpiresDate.Should().BeCloseTo(newRefreshToken.ExpiresDate, TimeSpan.FromSeconds(1));
        _mockTokenRepo.Verify(r => r.UpdateAsync(existingToken), Times.Once);
        _mockTokenRepo.Verify(r => r.AddAsync(It.IsAny<RefreshToken>()), Times.Never);
    }

    #endregion

    #region Refresh Token Tests

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "UserAuthService")]
    public async Task Refresh_WithValidToken_ReturnsNewTokens()
    {
        // Arrange
        string refreshTokenValue = "valid-refresh-token";
        var storedToken = new RefreshToken
        {
            Id = 1,
            Token = refreshTokenValue,
            UserId = 1,
            DeviceId = "device-001",
            DeviceName = "My Laptop",
            ExpiresDate = DateTime.UtcNow.AddDays(7),
            IsCanceled = false,
            User = new User
            {
                Id = 1,
                Username = "testuser",
                Role = UserRoles.Guest
            }
        };

        var newRefreshToken = new RefreshToken
        {
            Token = "new-refresh-token",
            UserId = storedToken.UserId,
            DeviceId = storedToken.DeviceId,
            DeviceName = storedToken.DeviceName,
            ExpiresDate = DateTime.UtcNow.AddDays(7)
        };

        _mockTokenRepo.Setup(r => r.GetTokenByTokenStringAsync(refreshTokenValue))
                      .ReturnsAsync(storedToken);
        _mockTokenService.Setup(s => s.GenerateRefreshToken(storedToken.DeviceId, storedToken.DeviceName))
                         .Returns(newRefreshToken);
        _mockTokenService.Setup(s => s.GenerateAccessToken(storedToken.User.Id, storedToken.User.Username, storedToken.User.Role!))
                         .Returns("new-access-token");
        _mockTokenRepo.Setup(r => r.UpdateAsync(It.IsAny<RefreshToken>()))
                      .Returns(Task.CompletedTask);
        _mockTokenRepo.Setup(r => r.SaveChangesAsync())
                      .Returns(Task.CompletedTask);

        // Act
        var result = await _service.Refresh(refreshTokenValue);

        // Assert
        result.AccessToken.Should().Be("new-access-token");
        result.RefreshToken.Should().Be("new-refresh-token");
        storedToken.Token.Should().Be("new-refresh-token");
        storedToken.IsCanceled.Should().BeFalse();
        _mockTokenRepo.Verify(r => r.UpdateAsync(storedToken), Times.Exactly(2)); // Once to cancel, once to update
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "UserAuthService")]
    public async Task Refresh_WithExpiredToken_ThrowsUnauthorized()
    {
        // Arrange
        string refreshTokenValue = "expired-token";
        var storedToken = new RefreshToken
        {
            Id = 1,
            Token = refreshTokenValue,
            ExpiresDate = DateTime.UtcNow.AddDays(-1), // Expired
            IsCanceled = false
        };

        _mockTokenRepo.Setup(r => r.GetTokenByTokenStringAsync(refreshTokenValue))
                      .ReturnsAsync(storedToken);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await _service.Refresh(refreshTokenValue)
        );
        _mockTokenRepo.Verify(r => r.UpdateAsync(It.IsAny<RefreshToken>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "UserAuthService")]
    public async Task Refresh_WithCanceledToken_ThrowsUnauthorized()
    {
        // Arrange
        string refreshTokenValue = "canceled-token";
        var storedToken = new RefreshToken
        {
            Id = 1,
            Token = refreshTokenValue,
            ExpiresDate = DateTime.UtcNow.AddDays(7),
            IsCanceled = true // Canceled
        };

        _mockTokenRepo.Setup(r => r.GetTokenByTokenStringAsync(refreshTokenValue))
                      .ReturnsAsync(storedToken);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await _service.Refresh(refreshTokenValue)
        );
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "UserAuthService")]
    public async Task Refresh_WithInvalidToken_ThrowsUnauthorized()
    {
        // Arrange
        string refreshTokenValue = "invalid-token";

        _mockTokenRepo.Setup(r => r.GetTokenByTokenStringAsync(refreshTokenValue))
                      .ReturnsAsync((RefreshToken?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await _service.Refresh(refreshTokenValue)
        );
    }

    #endregion

    #region Logout Tests

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "UserAuthService")]
    public async Task Logout_WithValidDevice_CancelsToken()
    {
        // Arrange
        int userId = 1;
        string deviceId = "device-001";
        var activeToken = new RefreshToken
        {
            Id = 1,
            UserId = userId,
            DeviceId = deviceId,
            IsCanceled = false,
            ExpiresDate = DateTime.UtcNow.AddDays(7)
        };

        _mockTokenRepo.Setup(r => r.GetByUserAndDeviceAsync(userId, deviceId))
                      .ReturnsAsync(activeToken);
        _mockTokenRepo.Setup(r => r.UpdateAsync(It.IsAny<RefreshToken>()))
                      .Returns(Task.CompletedTask);
        _mockTokenRepo.Setup(r => r.SaveChangesAsync())
                      .Returns(Task.CompletedTask);

        // Act
        await _service.CancelDeviceToken(userId, deviceId);

        // Assert
        activeToken.IsCanceled.Should().BeTrue();
        _mockTokenRepo.Verify(r => r.UpdateAsync(activeToken), Times.Once);
        _mockTokenRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "UserAuthService")]
    public async Task Logout_WithEmptyDeviceId_DoesNothing()
    {
        // Arrange
        int userId = 1;
        string deviceId = "";

        // Act
        await _service.CancelDeviceToken(userId, deviceId);

        // Assert
        _mockTokenRepo.Verify(r => r.GetByUserAndDeviceAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        _mockTokenRepo.Verify(r => r.UpdateAsync(It.IsAny<RefreshToken>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "UserAuthService")]
    public async Task Logout_WithNonexistentToken_DoesNothing()
    {
        // Arrange
        int userId = 1;
        string deviceId = "nonexistent-device";

        _mockTokenRepo.Setup(r => r.GetByUserAndDeviceAsync(userId, deviceId))
                      .ReturnsAsync((RefreshToken?)null);

        // Act
        await _service.CancelDeviceToken(userId, deviceId);

        // Assert
        _mockTokenRepo.Verify(r => r.UpdateAsync(It.IsAny<RefreshToken>()), Times.Never);
        _mockTokenRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "UserAuthService")]
    public async Task Logout_WithAlreadyCanceledToken_DoesNotUpdate()
    {
        // Arrange
        int userId = 1;
        string deviceId = "device-001";
        var canceledToken = new RefreshToken
        {
            Id = 1,
            UserId = userId,
            DeviceId = deviceId,
            IsCanceled = true, // Already canceled
            ExpiresDate = DateTime.UtcNow.AddDays(7)
        };

        _mockTokenRepo.Setup(r => r.GetByUserAndDeviceAsync(userId, deviceId))
                      .ReturnsAsync(canceledToken);

        // Act
        await _service.CancelDeviceToken(userId, deviceId);

        // Assert
        _mockTokenRepo.Verify(r => r.UpdateAsync(It.IsAny<RefreshToken>()), Times.Never);
        _mockTokenRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    #endregion
}
