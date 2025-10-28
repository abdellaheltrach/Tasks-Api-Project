namespace LoginApp.Business.DTOs.Task
{
    public class TaskCreateDTO
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int TaskStatusId { get; set; } = 1; // default Pending
        public DateTime? DueDate { get; set; } = null;


    }
}
