using LoginApp.DataAccess.Data;
using LoginApp.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using TaskStatusEntity = LoginApp.DataAccess.Entities.TaskStatus;

namespace LoginAppTest.Helpers;

/// <summary>
/// Factory class for creating in-memory database contexts for testing.
/// Provides isolated database instances with optional seed data.
/// </summary>
public static class TestDbContextFactory
{
    /// <summary>
    /// Creates a new AppDbContext with an in-memory database.
    /// Each call creates a unique database to ensure test isolation.
    /// </summary>
    public static AppDbContext CreateInMemoryContext(string databaseName = "TestDb")
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"{databaseName}_{Guid.NewGuid()}")
            .Options;

        var context = new AppDbContext(options);
        
        // Ensure the database is created
        context.Database.EnsureCreated();
        
        return context;
    }

    /// <summary>
    /// Creates a context with pre-seeded test data.
    /// Useful for tests that need existing data.
    /// </summary>
    public static AppDbContext CreateContextWithSeedData()
    {
        var context = CreateInMemoryContext("SeedTestDb");
        SeedTestData(context);
        return context;
    }

    /// <summary>
    /// Seeds the database with standard test data.
    /// </summary>
    private static void SeedTestData(AppDbContext context)
    {
        // Seed TaskStatus
        context.Set<TaskStatusEntity>().AddRange(
            new TaskStatusEntity { Id = 1, Name = "Pending" },
            new TaskStatusEntity { Id = 2, Name = "In Progress" },
            new TaskStatusEntity { Id = 3, Name = "Done" }
        );

        context.SaveChanges();
    }

    /// <summary>
    /// Creates a context for a specific test with custom seed data.
    /// </summary>
    public static AppDbContext CreateContextWithCustomData(Action<AppDbContext> seedAction)
    {
        var context = CreateInMemoryContext();
        
        // Add default task statuses
        context.Set<TaskStatusEntity>().AddRange(
            new TaskStatusEntity { Id = 1, Name = "Pending" },
            new TaskStatusEntity { Id = 2, Name = "In Progress" },
            new TaskStatusEntity { Id = 3, Name = "Done" }
        );
        
        context.SaveChanges();
        
        // Apply custom seed data
        seedAction(context);
        context.SaveChanges();
        
        return context;
    }
}
