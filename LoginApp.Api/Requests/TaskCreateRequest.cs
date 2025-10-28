namespace LoginApp.Api.Requests
{
    public class TaskCreateRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int TaskStatusId { get; set; }
        public DateTime? DueDate { get; set; }
    }


    public class TaskUpdateRequest
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int TaskStatusId { get; set; }
        public DateTime? DueDate { get; set; }
    }


}
