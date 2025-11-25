using System.ComponentModel.DataAnnotations;

namespace LoginApp.Business.DTOs.Task
{
    public class TaskUpdateDTO
    {
        public int Id { get; set; }             // Required to identify which task to update

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }
        public int TaskStatusId { get; set; }
        public DateTime? DueDate { get; set; } = null;  // Optional due date
    }
}
