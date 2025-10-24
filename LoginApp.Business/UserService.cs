using LoginApp.DataAccess.Entities;
using LoginApp.DataAccess.Repositories;
using System.Data;

namespace LoginApp.Business;

public class UserService
{
    private readonly IUserRepository _UserRepository;

    public UserService(IUserRepository userRepository)
    {
        this._UserRepository = userRepository;
    }

    public bool Register(string UserName, string Password)
    {
        if (_UserRepository.FindUserByUserName(UserName) != null) //User Name already taken
        {
            return false;
        }

        var NewUser = new User {
            Role = "Guest",
            Username = UserName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password)
        };

        _UserRepository.AddUser(NewUser);
        _UserRepository.SaveChanges();  
        return true;

    }

    public bool Login(string UserName, string Password, out string role)
    {

        role = "Guest"; 


        var User = _UserRepository.FindUserByUserName(UserName);

        if (User == null) //No UserName match
        {
            return false;
        }

        if (BCrypt.Net.BCrypt.Verify(Password, User.PasswordHash))
        {
            role = User.Role;
            return true;
        }



        return false;


    }


}
