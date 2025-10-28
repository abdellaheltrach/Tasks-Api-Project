using LoginApp.Business.DTOs.Task;
using LoginApp.Business.Services.Interfaces;
using LoginApp.DataAccess.Entities;
using LoginApp.DataAccess.Repositories.Interfaces;
using System.Data;

namespace LoginApp.Business.Services;

public class TaskService : ITaskService
{
    private readonly ITaskItemRepository _taskRepo;

    public TaskService(ITaskItemRepository taskRepo)
    {
        _taskRepo = taskRepo;
    }



    public async Task<IEnumerable<TaskReadDTO>> GetAllTasksOfUserAsync(int userId)
    {
        var tasks = await _taskRepo.GetAllTasksOfUserAsync(userId);

        return tasks.Select(t => new TaskReadDTO
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            CreatedAt = t.CreatedAt,
            DueDate = t.DueDate,
            StatusName = t.Status?.Name ?? "",
            Username = t.User?.Username ?? "",
            IsDeleted = t.IsDeleted
        });
    }

    public async Task<TaskReadDTO?> GetTaskByIdAsync(int id)
    {
        var task = await _taskRepo.GetTaskByIdAsync(id);
        if (task == null) return null;

        return new TaskReadDTO
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            CreatedAt = task.CreatedAt,
            DueDate = task.DueDate,
            StatusName = task.Status?.Name ?? "",
            Username = task.User?.Username ?? "",
            IsDeleted = task.IsDeleted
        };
    }

    public async Task AddTaskAsync(int UserId, TaskCreateDTO dto)
    {
        var task = new TaskItem
        {
            Title = dto.Title,
            Description = dto.Description,
            CreatedAt = DateTime.Now,
            DueDate = dto.DueDate,
            UserId = UserId,
            TaskStatusId = dto.TaskStatusId
        };

        await _taskRepo.AddAsync(task);
        await _taskRepo.SaveAsync();
    }

    public async Task UpdateTaskAsync(TaskUpdateDTO dto)
    {
        var task = await _taskRepo.GetTaskByIdAsync(dto.Id);
        if (task == null) return;

        task.Title = dto.Title;
        task.Description = dto.Description;
        task.DueDate = dto.DueDate;
        task.TaskStatusId = dto.TaskStatusId;

        await _taskRepo.UpdateAsync(task);
        await _taskRepo.SaveAsync();
    }

    public async Task SoftDeleteTaskAsync(int id)
    {
        var task = await _taskRepo.GetTaskByIdAsync(id);
        if (task == null) return;

        await _taskRepo.SoftDeleteAsync(task);
        await _taskRepo.SaveAsync();
    }
}
