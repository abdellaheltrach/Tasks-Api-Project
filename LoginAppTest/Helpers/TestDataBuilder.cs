using Bogus;
using LoginApp.Business.Constants;
using LoginApp.Business.Helpers;
using LoginApp.DataAccess.Entities;
using TaskStatusEntity = LoginApp.DataAccess.Entities.TaskStatus;

namespace LoginAppTest.Helpers;

/// <summary>
/// Fluent builder for creating test entities using Bogus fake data library.
/// Provides realistic test data with sensible defaults.
/// </summary>
public static class TestDataBuilder
{
    private static readonly Faker _faker = new Faker();

    /// <summary>
    /// Creates a test user with realistic data.
    /// </summary>
    public static class Users
    {
        public static User CreateGuestUser(string? username = null, string? password = null)
        {
            return new User
            {
                Id = _faker.Random.Int(1, 10000),
                Username = username ?? _faker.Internet.UserName(),
                PasswordHash = clsPasswordHasher.Hash(password ?? "TestPassword123"),
                Role = UserRoles.Guest
            };
        }

        public static User CreateAdminUser(string? username = null, string? password = null)
        {
            return new User
            {
                Id = _faker.Random.Int(1, 10000),
                Username = username ?? _faker.Internet.UserName(),
                PasswordHash = clsPasswordHasher.Hash(password ?? "AdminPassword123"),
                Role = UserRoles.Admin
            };
        }

        public static User CreateUserWithId(int id, string? username = null)
        {
            return new User
            {
                Id = id,
                Username = username ?? _faker.Internet.UserName(),
                PasswordHash = clsPasswordHasher.Hash("TestPassword123"),
                Role = UserRoles.Guest
            };
        }
    }

    /// <summary>
    /// Creates test tasks with realistic data.
    /// </summary>
    public static class Tasks
    {
        public static TaskItem CreateTask(int userId, int statusId = 1, bool isDeleted = false)
        {
            return new TaskItem
            {
                Id = _faker.Random.Int(1, 10000),
                Title = _faker.Lorem.Sentence(3),
                Description = _faker.Lorem.Paragraph(),
                UserId = userId,
                TaskStatusId = statusId,
                CreatedAt = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(_faker.Random.Int(1, 30)),
                IsDeleted = isDeleted
            };
        }

        public static TaskItem CreateTaskWithId(int id, int userId, int statusId = 1)
        {
            return new TaskItem
            {
                Id = id,
                Title = _faker.Lorem.Sentence(3),
                Description = _faker.Lorem.Paragraph(),
                UserId = userId,
                TaskStatusId = statusId,
                CreatedAt = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(7),
                IsDeleted = false
            };
        }

        public static List<TaskItem> CreateMultipleTasks(int count, int userId, int statusId = 1)
        {
            var tasks = new List<TaskItem>();
            for (int i = 0; i < count; i++)
            {
                tasks.Add(CreateTask(userId, statusId));
            }
            return tasks;
        }
    }

    /// <summary>
    /// Creates test refresh tokens.
    /// </summary>
    public static class RefreshTokens
    {
        public static RefreshToken CreateToken(int userId, string? deviceId = null, bool isExpired = false, bool isCanceled = false)
        {
            return new RefreshToken
            {
                Id = _faker.Random.Int(1, 10000),
                Token = Guid.NewGuid().ToString(),
                UserId = userId,
                DeviceId = deviceId ?? Guid.NewGuid().ToString(),
                DeviceName = _faker.Lorem.Word(),
                ExpiresDate = isExpired ? DateTime.UtcNow.AddDays(-1) : DateTime.UtcNow.AddDays(7),
                IsCanceled = isCanceled
            };
        }

        public static RefreshToken CreateActiveToken(int userId, string deviceId)
        {
            return CreateToken(userId, deviceId, isExpired: false, isCanceled: false);
        }

        public static RefreshToken CreateExpiredToken(int userId, string deviceId)
        {
            return CreateToken(userId, deviceId, isExpired: true, isCanceled: false);
        }

        public static RefreshToken CreateCanceledToken(int userId, string deviceId)
        {
            return CreateToken(userId, deviceId, isExpired: false, isCanceled: true);
        }
    }

    /// <summary>
    /// Creates test task statuses.
    /// </summary>
    public static class TaskStatuses
    {
        public static TaskStatusEntity Pending() => new TaskStatusEntity { Id = 1, Name = "Pending" };
        public static TaskStatusEntity InProgress() => new TaskStatusEntity { Id = 2, Name = "In Progress" };
        public static TaskStatusEntity Done() => new TaskStatusEntity { Id = 3, Name = "Done" };

        public static List<TaskStatusEntity> AllStatuses() => new()
        {
            Pending(),
            InProgress(),
            Done()
        };
    }
}
