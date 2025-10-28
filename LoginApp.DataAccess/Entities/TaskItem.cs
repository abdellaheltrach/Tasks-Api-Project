namespace LoginApp.DataAccess.Entities
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? DueDate { get; set; }

        public bool IsDeleted { get; set; } = false; //soft delete

        //relation with user

        public int? UserId { get; set; }
        public User User { get; set; } = null!;

        // relation with taskStatus

        public int TaskStatusId { get; set; }
        public TaskStatus Status { get; set; } = null!;

    }
}
