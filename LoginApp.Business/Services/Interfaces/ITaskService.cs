using LoginApp.Business.DTOs.Task;
using LoginApp.DataAccess.Entities;

namespace LoginApp.Business.Services.Interfaces
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskReadDTO>> GetAllTasksOfUserAsync(int userId);
        Task<TaskReadDTO?> GetTaskByIdAsync(int id);
        Task AddTaskAsync(int UserId, TaskCreateDTO dto);
        Task UpdateTaskAsync(TaskUpdateDTO dto);
        Task SoftDeleteTaskAsync(int id);
    }

}
