using LoginApp.DataAccess.Data;
using LoginApp.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginApp.DataAccess.Repositories
{
    public interface IUserRepository
    {
        public void AddUser(User user);
        public User? FindUserByUserName(string UserName);
        public void SaveChanges();
    }
}
