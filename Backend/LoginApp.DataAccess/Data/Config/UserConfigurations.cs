using LoginApp.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginApp.DataAccess.Data.Config
{
    public class UserConfigurations : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // Table name (optional, defaults to "Users")
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);


            // Seed Admin User
            builder.HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    PasswordHash = "$2a$11$0jjaKSFycpBuRCPCjpFifeb37evdVrYq98U0aT8T3d7pavQUh5xx6",
                    Role = "Admin"
                },
                new User
                {
                    Id = 2,
                    Username = "guest",
                    PasswordHash = "$2a$11$bn3QqVz07vRjHP.qleORVec0EP0TgDbW583IXIy1axeFiIy/rtkuC",
                    Role = "Guest"
                }
            );
        }
    }
}
