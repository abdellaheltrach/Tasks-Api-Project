//using LoginApp.Business.DTOs.login;
//using LoginApp.Business.Helpers;
//using LoginApp.Business.Services.Interfaces;
//using LoginApp.DataAccess.Entities;
//using LoginApp.DataAccess.Repositories;
//using LoginApp.DataAccess.Repositories.Interfaces;
//using Microsoft.Extensions.Configuration;
//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;

//namespace LoginApp.Business.Services;

//public class UserService : IUserService
//{
//    private readonly IUserRepository _UserRepository;

//    public UserService(IUserRepository userRepository)
//    {
//        _UserRepository = userRepository;
//    }

//    public bool Register(RegisterDTO requestDto)
//    {
//        if (_UserRepository.FindUserByUserName(requestDto.Username) != null) //User Name already taken
//        {
//            return false;
//        }

//        var NewUser = new User {
//            Role = "Guest",
//            Username = requestDto.Username,
//            PasswordHash =  clsPasswordHasher.Hash(requestDto.Password)
//        };

//        _UserRepository.AddUser(NewUser);
//        _UserRepository.SaveChanges();  
//        return true;

//    }

//    public bool Login(LoginDTO requestDto, out string role)
//    {

//        role = "Guest"; 


//        var User = _UserRepository.FindUserByUserName(requestDto.Username);

//        if (User == null) //No UserName match
//        {
//            return false;
//        }

//        if (clsPasswordHasher.Verify(requestDto.Password, User.PasswordHash))
//        {
//            role = User.Role;
//            return true;
//        }
//        return false;


//    }


//}


using LoginApp.Business.DTOs.login;
using LoginApp.Business.Helpers;
using LoginApp.Business.Services.Interfaces;
using LoginApp.DataAccess.Entities;
using LoginApp.DataAccess.Repositories;
using LoginApp.DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;

public class UserService : IUserService
{
    private readonly IUserRepository _UserRepository;
    private readonly ITokenService _Token;

    public UserService(IUserRepository userRepository, ITokenService Token)
    {
        _UserRepository = userRepository;
        _Token = Token;
    }

    public bool Register(RegisterDTO requestDto)
    {
        if (_UserRepository.FindUserByUserName(requestDto.Username) != null) //User Name already taken
        {
            return false;
        }

        var NewUser = new User
        {
            Role = "Guest",
            Username = requestDto.Username,
            PasswordHash = clsPasswordHasher.Hash(requestDto.Password)
        };

        _UserRepository.AddUser(NewUser);
        _UserRepository.SaveChanges();
        return true;

    }



    public (bool Success, string? Token, string? Role) Login(LoginDTO requestDto)
    {
        var user = _UserRepository.FindUserByUserName(requestDto.Username);

        if (user == null) //No UserName match
            return (false, null, null);

        if (clsPasswordHasher.Verify(requestDto.Password, user.PasswordHash))
        {
            var token = _Token.GenerateToken(user.Id ,user.Username, user.Role);
            return (true, token, user.Role);
        }

        return (false, null, null);
    }



}
