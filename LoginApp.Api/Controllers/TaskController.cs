using LoginApp.Business.DTOs.Task;
using LoginApp.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LoginApp.Api.Controllers
{
    [ApiController]
    [Route("api/Task")]
    [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }


        private int GetUserIdFromClaims()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var id))
            {
                throw new UnauthorizedAccessException("User id claim missing or invalid");
            }
            return id;
        }



        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] TaskCreateDTO request)
        {
            var userId = GetUserIdFromClaims();

            int UserId = userId;

            await _taskService.AddTaskAsync(UserId, request);
            return Ok();
        }



        [HttpGet]
        public async Task<IActionResult> GetAllTasks()
        {
            var userId = GetUserIdFromClaims();
            var tasks = await _taskService.GetAllTasksOfUserAsync(userId);
            return Ok(tasks);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            var userId = GetUserIdFromClaims();
            var task = await _taskService.GetTaskByIdAsync(id, userId);
            if (task == null) return NotFound();

            return Ok(task);
        }




        [HttpPut]
        public async Task<IActionResult> UpdateTask([FromBody] TaskUpdateDTO request)
        {
            var userId = GetUserIdFromClaims();
            var success = await _taskService.UpdateTaskAsync(userId, request);
            if (!success) return NotFound();

            return Ok();
        }


        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateTaskStatus(int id, [FromBody] TaskStatusUpdateDTO request)
        {
            if (id != request.TaskId) return BadRequest("Task ID mismatch");

            var userId = GetUserIdFromClaims();
            var success = await _taskService.UpdateTaskStatusAsync(userId, request);
            if (!success) return NotFound();

            return Ok();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var userId = GetUserIdFromClaims();
            var success = await _taskService.SoftDeleteTaskAsync(id, userId);
            if (!success) return NotFound();

            return Ok();
        }


    }
}
