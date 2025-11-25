using FluentAssertions;
using LoginApp.Business.DTOs;
using LoginApp.Business.DTOs.Task;
using LoginApp.Business.Services;
using LoginApp.DataAccess.Entities;
using LoginApp.DataAccess.Repositories;
using LoginApp.DataAccess.Repositories.Interfaces;
using LoginAppTest.Helpers;
using Moq;
using TaskStatusEntity = LoginApp.DataAccess.Entities.TaskStatus;

namespace LoginAppTest.Unit.Services;

/// <summary>
/// Unit tests for TaskService covering all CRUD operations and authorization logic.
/// Uses Moq to isolate service layer from data access layer.
/// </summary>
public class TaskServiceTests
{
    private readonly Mock<ITaskItemRepository> _mockRepo;
    private readonly TaskService _service;

    public TaskServiceTests()
    {
        _mockRepo = new Mock<ITaskItemRepository>();
        _service = new TaskService(_mockRepo.Object);
    }

    #region GetAllTasksOfUserAsync Tests

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "TaskService")]
    public async Task GetAllTasksOfUserAsync_WithTasks_ReturnsMappedDTOs()
    {
        // Arrange
        int userId = 1;
        var tasks = new List<TaskItem>
        {
            new() 
            { 
                Id = 1, 
                Title = "Task 1", 
                Description = "Description 1",
                UserId = userId,
                TaskStatusId = 1,
                Status = new TaskStatusEntity { Id = 1, Name = "Pending" },
                User = new User { Id = userId, Username = "testuser" },
                CreatedAt = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(7),
                IsDeleted = false
            },
            new() 
            { 
                Id = 2, 
                Title = "Task 2", 
                Description = "Description 2",
                UserId = userId,
                TaskStatusId = 2,
                Status = new TaskStatusEntity { Id = 2, Name = "In Progress" },
                User = new User { Id = userId, Username = "testuser" },
                CreatedAt = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(5),
                IsDeleted = false
            }
        };

        _mockRepo.Setup(r => r.GetAllTasksOfUserAsync(userId))
                 .ReturnsAsync(tasks);

        // Act
        var result = await _service.GetAllTasksOfUserAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().Title.Should().Be("Task 1");
        result.First().StatusName.Should().Be("Pending");
        result.Last().Title.Should().Be("Task 2");
        result.Last().StatusName.Should().Be("In Progress");
        _mockRepo.Verify(r => r.GetAllTasksOfUserAsync(userId), Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "TaskService")]
    public async Task GetAllTasksOfUserAsync_WithNoTasks_ReturnsEmptyList()
    {
        // Arrange
        int userId = 1;
        _mockRepo.Setup(r => r.GetAllTasksOfUserAsync(userId))
                 .ReturnsAsync(new List<TaskItem>());

        // Act
        var result = await _service.GetAllTasksOfUserAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        _mockRepo.Verify(r => r.GetAllTasksOfUserAsync(userId), Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "TaskService")]
    public async Task GetAllTasksOfUserAsync_HandlesNullStatusGracefully()
    {
        // Arrange
        int userId = 1;
        var tasks = new List<TaskItem>
        {
            new() 
            { 
                Id = 1, 
                Title = "Task 1",
                UserId = userId,
                Status = null!, // Null status
                User = new User { Username = "testuser" }
            }
        };

        _mockRepo.Setup(r => r.GetAllTasksOfUserAsync(userId))
                 .ReturnsAsync(tasks);

        // Act
        var result = await _service.GetAllTasksOfUserAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.First().StatusName.Should().BeEmpty();
    }

    #endregion

    #region GetTaskByIdAsync Tests

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "TaskService")]
    public async Task GetTaskByIdAsync_WhenUserOwnsTask_ReturnsTask()
    {
        // Arrange
        int taskId = 1;
        int userId = 1;
        var task = new TaskItem
        {
            Id = taskId,
            Title = "My Task",
            Description = "My Description",
            UserId = userId,
            TaskStatusId = 1,
            Status = new TaskStatusEntity { Id = 1, Name = "Pending" },
            User = new User { Id = userId, Username = "testuser" },
            CreatedAt = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(7),
            IsDeleted = false
        };

        _mockRepo.Setup(r => r.GetTaskByIdAsync(taskId))
                 .ReturnsAsync(task);

        // Act
        var result = await _service.GetTaskByIdAsync(taskId, userId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(taskId);
        result.Title.Should().Be("My Task");
        result.Username.Should().Be("testuser");
        _mockRepo.Verify(r => r.GetTaskByIdAsync(taskId), Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "TaskService")]
    public async Task GetTaskByIdAsync_WhenUserDoesNotOwnTask_ReturnsNull()
    {
        // Arrange
        int taskId = 1;
        int requestingUserId = 2; // Different user
        var task = new TaskItem
        {
            Id = taskId,
            Title = "Someone Else's Task",
            UserId = 1, // Owner is user 1
            Status = new TaskStatusEntity { Name = "Pending" }
        };

        _mockRepo.Setup(r => r.GetTaskByIdAsync(taskId))
                 .ReturnsAsync(task);

        // Act
        var result = await _service.GetTaskByIdAsync(taskId, requestingUserId);

        // Assert
        result.Should().BeNull();
        _mockRepo.Verify(r => r.GetTaskByIdAsync(taskId), Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "TaskService")]
    public async Task GetTaskByIdAsync_WhenTaskNotFound_ReturnsNull()
    {
        // Arrange
        int taskId = 999;
        int userId = 1;

        _mockRepo.Setup(r => r.GetTaskByIdAsync(taskId))
                 .ReturnsAsync((TaskItem?)null);

        // Act
        var result = await _service.GetTaskByIdAsync(taskId, userId);

        // Assert
        result.Should().BeNull();
        _mockRepo.Verify(r => r.GetTaskByIdAsync(taskId), Times.Once);
    }

    #endregion

    #region AddTaskAsync Tests

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "TaskService")]
    public async Task AddTaskAsync_WithValidData_CreatesTask()
    {
        // Arrange
        int userId = 1;
        var dto = new TaskCreateDTO
        {
            Title = "New Task",
            Description = "New Description",
            DueDate = DateTime.UtcNow.AddDays(7),
            TaskStatusId = 1
        };

        _mockRepo.Setup(r => r.AddAsync(It.IsAny<TaskItem>()))
                 .Returns(Task.CompletedTask);
        _mockRepo.Setup(r => r.SaveAsync())
                 .Returns(Task.CompletedTask);

        // Act
        await _service.AddTaskAsync(userId, dto);

        // Assert
        _mockRepo.Verify(r => r.AddAsync(It.Is<TaskItem>(t =>
            t.Title == dto.Title &&
            t.Description == dto.Description &&
            t.UserId == userId &&
            t.TaskStatusId == dto.TaskStatusId &&
            t.DueDate == dto.DueDate
        )), Times.Once);
        _mockRepo.Verify(r => r.SaveAsync(), Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "TaskService")]
    public async Task AddTaskAsync_SetsCreatedAtTimestamp()
    {
        // Arrange
        int userId = 1;
        var dto = new TaskCreateDTO
        {
            Title = "Task with Timestamp",
            Description = "Description",
            TaskStatusId = 1
        };

        DateTime capturedCreatedAt = default;
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<TaskItem>()))
                 .Callback<TaskItem>(t => capturedCreatedAt = t.CreatedAt)
                 .Returns(Task.CompletedTask);
        _mockRepo.Setup(r => r.SaveAsync())
                 .Returns(Task.CompletedTask);

        // Act
        await _service.AddTaskAsync(userId, dto);

        // Assert
        capturedCreatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        _mockRepo.Verify(r => r.SaveAsync(), Times.Once);
    }

    #endregion

    #region UpdateTaskAsync Tests

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "TaskService")]
    public async Task UpdateTaskAsync_WhenUserOwnsTask_UpdatesSuccessfully()
    {
        // Arrange
        int userId = 1;
        var existingTask = new TaskItem
        {
            Id = 1,
            Title = "Old Title",
            Description = "Old Description",
            UserId = userId,
            TaskStatusId = 1,
            DueDate = DateTime.UtcNow.AddDays(5)
        };

        var updateDto = new TaskUpdateDTO
        {
            Id = 1,
            Title = "Updated Title",
            Description = "Updated Description",
            TaskStatusId = 2,
            DueDate = DateTime.UtcNow.AddDays(10)
        };

        _mockRepo.Setup(r => r.GetTaskByIdAsync(existingTask.Id))
                 .ReturnsAsync(existingTask);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<TaskItem>()))
                 .Returns(Task.CompletedTask);
        _mockRepo.Setup(r => r.SaveAsync())
                 .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateTaskAsync(userId, updateDto);

        // Assert
        result.Should().BeTrue();
        existingTask.Title.Should().Be("Updated Title");
        existingTask.Description.Should().Be("Updated Description");
        existingTask.TaskStatusId.Should().Be(2);
        _mockRepo.Verify(r => r.UpdateAsync(existingTask), Times.Once);
        _mockRepo.Verify(r => r.SaveAsync(), Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "TaskService")]
    public async Task UpdateTaskAsync_WhenUserDoesNotOwnTask_ReturnsFalse()
    {
        // Arrange
        int requestingUserId = 2;
        var existingTask = new TaskItem
        {
            Id = 1,
            Title = "Task",
            UserId = 1 // Different owner
        };

        var updateDto = new TaskUpdateDTO
        {
            Id = 1,
            Title = "Attempted Update"
        };

        _mockRepo.Setup(r => r.GetTaskByIdAsync(existingTask.Id))
                 .ReturnsAsync(existingTask);

        // Act
        var result = await _service.UpdateTaskAsync(requestingUserId, updateDto);

        // Assert
        result.Should().BeFalse();
        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<TaskItem>()), Times.Never);
        _mockRepo.Verify(r => r.SaveAsync(), Times.Never);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "TaskService")]
    public async Task UpdateTaskAsync_WhenTaskNotFound_ReturnsFalse()
    {
        // Arrange
        int userId = 1;
        var updateDto = new TaskUpdateDTO { Id = 999, Title = "Update" };

        _mockRepo.Setup(r => r.GetTaskByIdAsync(999))
                 .ReturnsAsync((TaskItem?)null);

        // Act
        var result = await _service.UpdateTaskAsync(userId, updateDto);

        // Assert
        result.Should().BeFalse();
        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<TaskItem>()), Times.Never);
    }

    #endregion

    #region UpdateTaskStatusAsync Tests

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "TaskService")]
    public async Task UpdateTaskStatusAsync_WhenUserOwnsTask_UpdatesStatus()
    {
        // Arrange
        int userId = 1;
        var existingTask = new TaskItem
        {
            Id = 1,
            Title = "Task",
            UserId = userId,
            TaskStatusId = 1
        };

        var statusUpdate = new TaskStatusUpdateDTO
        {
            TaskId = 1,
            StatusId = 3
        };

        _mockRepo.Setup(r => r.GetTaskByIdAsync(1))
                 .ReturnsAsync(existingTask);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<TaskItem>()))
                 .Returns(Task.CompletedTask);
        _mockRepo.Setup(r => r.SaveAsync())
                 .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateTaskStatusAsync(userId, statusUpdate);

        // Assert
        result.Should().BeTrue();
        existingTask.TaskStatusId.Should().Be(3);
        _mockRepo.Verify(r => r.UpdateAsync(existingTask), Times.Once);
        _mockRepo.Verify(r => r.SaveAsync(), Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "TaskService")]
    public async Task UpdateTaskStatusAsync_WhenUserDoesNotOwnTask_ReturnsFalse()
    {
        // Arrange
        int requestingUserId = 2;
        var existingTask = new TaskItem
        {
            Id = 1,
            UserId = 1, // Different owner
            TaskStatusId = 1
        };

        var statusUpdate = new TaskStatusUpdateDTO
        {
            TaskId = 1,
            StatusId = 3
        };

        _mockRepo.Setup(r => r.GetTaskByIdAsync(1))
                 .ReturnsAsync(existingTask);

        // Act
        var result = await _service.UpdateTaskStatusAsync(requestingUserId, statusUpdate);

        // Assert
        result.Should().BeFalse();
        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<TaskItem>()), Times.Never);
    }

    #endregion

    #region SoftDeleteTaskAsync Tests

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "TaskService")]
    public async Task SoftDeleteTaskAsync_WhenUserOwnsTask_DeletesSuccessfully()
    {
        // Arrange
        int userId = 1;
        int taskId = 1;
        var existingTask = new TaskItem
        {
            Id = taskId,
            Title = "Task to Delete",
            UserId = userId,
            IsDeleted = false
        };

        _mockRepo.Setup(r => r.GetTaskByIdAsync(taskId))
                 .ReturnsAsync(existingTask);
        _mockRepo.Setup(r => r.SoftDeleteAsync(It.IsAny<TaskItem>()))
                 .Returns(Task.CompletedTask);
        _mockRepo.Setup(r => r.SaveAsync())
                 .Returns(Task.CompletedTask);

        // Act
        var result = await _service.SoftDeleteTaskAsync(taskId, userId);

        // Assert
        result.Should().BeTrue();
        _mockRepo.Verify(r => r.SoftDeleteAsync(existingTask), Times.Once);
        _mockRepo.Verify(r => r.SaveAsync(), Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "TaskService")]
    public async Task SoftDeleteTaskAsync_WhenUserDoesNotOwnTask_ReturnsFalse()
    {
        // Arrange
        int requestingUserId = 2;
        int taskId = 1;
        var existingTask = new TaskItem
        {
            Id = taskId,
            UserId = 1 // Different owner
        };

        _mockRepo.Setup(r => r.GetTaskByIdAsync(taskId))
                 .ReturnsAsync(existingTask);

        // Act
        var result = await _service.SoftDeleteTaskAsync(taskId, requestingUserId);

        // Assert
        result.Should().BeFalse();
        _mockRepo.Verify(r => r.SoftDeleteAsync(It.IsAny<TaskItem>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "TaskService")]
    public async Task SoftDeleteTaskAsync_WhenTaskNotFound_ReturnsFalse()
    {
        // Arrange
        int userId = 1;
        int taskId = 999;

        _mockRepo.Setup(r => r.GetTaskByIdAsync(taskId))
                 .ReturnsAsync((TaskItem?)null);

        // Act
        var result = await _service.SoftDeleteTaskAsync(taskId, userId);

        // Assert
        result.Should().BeFalse();
        _mockRepo.Verify(r => r.SoftDeleteAsync(It.IsAny<TaskItem>()), Times.Never);
    }

    #endregion
}
