using LoginApp.DataAccess.Entities;

namespace LoginApp.DataAccess.Repositories.Interfaces
{
    //public interface ITaskItemRepository
    //{
    //    TaskItem? GetTaskById(int id);

    //    IEnumerable<TaskItem> GetAllTasksOfUser(int userId);
    //    void Add(TaskItem task);
    //    void Update(TaskItem task);
    //    void SoftDelete(TaskItem task);
    //    void Save();
    //}



    public interface ITaskItemRepository
    {
        Task<TaskItem?> GetTaskByIdAsync(int id);

        Task<IEnumerable<TaskItem>> GetAllTasksOfUserAsync(int userId);
        Task AddAsync(TaskItem task);
        Task UpdateAsync(TaskItem task);
        Task SoftDeleteAsync(TaskItem task);
        Task SaveAsync();
    }
}
