using LoginApp.Business.DTOs.Task;
using LoginApp.DataAccess.Entities;

namespace LoginApp.Business.Services.Interfaces
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskReadDTO>> GetAllTasksOfUserAsync(int userId);
        Task<TaskReadDTO?> GetTaskByIdAsync(int id, int userId);
        Task AddTaskAsync(int UserId, TaskCreateDTO dto);
        Task<bool> UpdateTaskAsync(int userId, TaskUpdateDTO dto);
        Task<bool> UpdateTaskStatusAsync(int userId, TaskStatusUpdateDTO dto);
        Task<bool> SoftDeleteTaskAsync(int id, int userId);
    }

}
