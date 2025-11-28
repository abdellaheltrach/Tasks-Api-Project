using System.ComponentModel.DataAnnotations;

namespace LoginApp.Business.DTOs.Task
{
    public class TaskCreateDTO
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public int TaskStatusId { get; set; }
    }
}
