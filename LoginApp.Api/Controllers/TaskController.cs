using LoginApp.Business.DTOs.Task;
using LoginApp.Business.Services;
using LoginApp.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LoginApp.Api.Controllers
{
    [ApiController]
    [Route("api/Task")]
    [Authorize] // require JWT for all endpoints
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        // helper: safely get user id from claims
        private int GetUserIdFromClaims()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var id))
            {
                throw new UnauthorizedAccessException("User id claim missing or invalid");
            }
            return id;
        }


        // POST: api/task
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] TaskCreateDTO request)
        {
            var userId = GetUserIdFromClaims();

            // Assign userId from claims, do not trust client
            int UserId = userId;

            await _taskService.AddTaskAsync(UserId, request);
            return Ok();
        }


        // GET: api/task
        [HttpGet]
        public async Task<IActionResult> GetAllTasks()
        {
            var userId = GetUserIdFromClaims();
            var tasks = await _taskService.GetAllTasksOfUserAsync(userId);
            return Ok(tasks);
        }

        // GET: api/task/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            var userId = GetUserIdFromClaims();
            var task = await _taskService.GetTaskByIdAsync(id, userId);
            if (task == null) return NotFound();
            
            return Ok(task);
        }



        // PUT: api/task
        [HttpPut]
        public async Task<IActionResult> UpdateTask([FromBody] TaskUpdateDTO request)
        {
            var userId = GetUserIdFromClaims();
            var success = await _taskService.UpdateTaskAsync(userId, request);
            if (!success) return NotFound();
            
            return Ok();
        }

        // PATCH: api/task/{id}/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateTaskStatus(int id, [FromBody] TaskStatusUpdateDTO request)
        {
            if (id != request.TaskId) return BadRequest("Task ID mismatch");

            var userId = GetUserIdFromClaims();
            var success = await _taskService.UpdateTaskStatusAsync(userId, request);
            if (!success) return NotFound();
            
            return Ok();
        }

        // DELETE: api/task/{id}
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
