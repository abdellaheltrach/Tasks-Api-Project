using LoginApp.DataAccess.Data;
using LoginApp.DataAccess.Entities;
using LoginApp.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace LoginApp.DataAccess.Repositories
{





    public class TaskRepository : ITaskItemRepository
    {
        private readonly AppDbContext _context;
        public TaskRepository(AppDbContext context) => _context = context;




        public async Task<TaskItem?> GetTaskByIdAsync(int id)
        {
            return await _context.Tasks
                .Where(t => t.Id == id && !t.IsDeleted)  
                .Include(t=> t.Status)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetAllTasksOfUserAsync(int userId)
            => await _context.Tasks.Where(t => t.UserId == userId).ToListAsync();

        public async Task AddAsync(TaskItem task) => await _context.Tasks.AddAsync(task);

        public Task UpdateAsync(TaskItem task)
        {
            _context.Tasks.Update(task);      // mark entity as modified
            return Task.CompletedTask;
        }

        public Task SoftDeleteAsync(TaskItem task)
        {
            task.Delete();

            _context.Tasks.Update(task);
            return Task.CompletedTask;
        }

        public async Task SaveAsync() => await _context.SaveChangesAsync();
    }


}
