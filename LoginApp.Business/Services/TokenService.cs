


using LoginApp.Business.Services.Interfaces;
using LoginApp.DataAccess.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    private readonly double _accessTokenExpireMinutes;
    private readonly double _refreshTokenDays;

    public TokenService(IConfiguration config)
    {
        _config = config;
        _accessTokenExpireMinutes = Convert.ToDouble(_config["Jwt:AccessTokenExpireMinutes"] ?? "15");
        _refreshTokenDays = Convert.ToDouble(_config["Jwt:RefreshTokenExpireDays"] ?? "7");
    }

    public string GenerateAccessToken(int userId, string username, string role)
    {
        var jwt = _config.GetSection("Jwt");  //Reads appsettings.json section Jwt


        //Converts your secret key into a cryptographic object used to sign the token
        //Symmetric means the same key is used to sign and validate the token.
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!)); //Create the Signing Key

        //Defines how to digitally sign the JWT
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);//Create Signing Credentials using HmacSha256 that its strong and common algorithm used for signing tokens

        var claims = new[] //User Identity Data The username and The role and id
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()), // <-- user id
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) //
        };



        //Create the Token Object
        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"], //who issued the token from appsettings.json
            audience: jwt["Audience"], //who is the audience of the token[who can use it], taken from appsettings.json
            claims: claims,            //who the user is and what their role is
            expires: DateTime.Now.AddMinutes(_accessTokenExpireMinutes), //how long the token is valid - from now till [ExpireMinutes from AppSettings] later
            signingCredentials: creds //how to verify the token’s authenticity
        );

        return new JwtSecurityTokenHandler().WriteToken(token); //Return the Encoded Token - Converts the token object into a compact string
    }

    public RefreshToken GenerateRefreshToken(string deviceId, string deviceName)
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return new RefreshToken
        {
            Token = Convert.ToBase64String(randomBytes),
            ExpiresDate = DateTime.UtcNow.AddDays(_refreshTokenDays),
            DeviceId = deviceId,
            DeviceName = deviceName
        };
    }
}