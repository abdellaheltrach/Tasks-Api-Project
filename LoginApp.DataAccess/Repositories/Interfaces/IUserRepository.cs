using LoginApp.DataAccess.Data;
using LoginApp.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginApp.DataAccess.Repositories.Interfaces
{
    public interface IUserRepository
    {
        public Task AddUserAsync(User user);
        public Task<User?> FindUserByUserNameAsync(string UserName);
        public Task SaveChangesAsync();
    }
}
