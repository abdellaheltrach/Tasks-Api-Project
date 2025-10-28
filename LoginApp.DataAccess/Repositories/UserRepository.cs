using LoginApp.DataAccess.Data;
using LoginApp.DataAccess.Entities;
using LoginApp.DataAccess.Repositories.Interfaces;


namespace LoginApp.DataAccess.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _Context;

        public UserRepository(AppDbContext context)
        {
            _Context = context;
        }

        public void AddUser(User user)
        {
             _Context.Users.Add(user);
        }

        public User? FindUserByUserName(string UserName)
        {
            return _Context.Users.FirstOrDefault(u => u.Username == UserName);
        }

        public void SaveChanges()
        {
             _Context.SaveChanges();
        }
    }
}
