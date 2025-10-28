using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginApp.DataAccess.Entities
{
    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public DateTime CreatingDate { get; set; } = DateTime.Now;

        public string Role { get; set; } = "Guest";

        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}
