using LoginApp.Business.DTOs.login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginApp.Business.Services.Interfaces
{
    public interface IUserService
    {
         bool Register(RegisterDTO request);
        (bool Success, string? Token, string? Role) Login(LoginDTO request);
    }
}
