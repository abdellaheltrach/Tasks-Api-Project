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
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null) return NotFound();
            // optionally enforce visibility (if deleted and not owner/admin)
            return Ok(task);
        }



        // PUT: api/task
        [HttpPut]
        public async Task<IActionResult> UpdateTask([FromBody] TaskUpdateDTO request)
        {
            // load current task to check owner
            var existing = await _taskService.GetTaskByIdAsync(request.Id);
            if (existing == null) return NotFound();

            var username = User.Identity?.Name ?? "";
            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && !string.Equals(existing.Username, username, StringComparison.OrdinalIgnoreCase))
                return Forbid();

            await _taskService.UpdateTaskAsync(request);
            return Ok();
        }

        // DELETE: api/task/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var existing = await _taskService.GetTaskByIdAsync(id);
            if (existing == null) return NotFound();

            await _taskService.SoftDeleteTaskAsync(id);
            return Ok();
        }


    }
}
