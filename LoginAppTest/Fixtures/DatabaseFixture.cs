using LoginApp.DataAccess.Data;

namespace LoginAppTest.Fixtures;

/// <summary>
/// xUnit test fixture for sharing database context across tests.
/// Implements IDisposable for proper cleanup.
/// </summary>
public class DatabaseFixture : IDisposable
{
    public AppDbContext Context { get; private set; }

    public DatabaseFixture()
    {
        Context = Helpers.TestDbContextFactory.CreateContextWithSeedData();
    }

    public void Dispose()
    {
        Context?.Dispose();
    }

    /// <summary>
    /// Resets the database to a clean state while keeping the fixture alive.
    /// </summary>
    public void Reset()
    {
        Context?.Dispose();
        Context = Helpers.TestDbContextFactory.CreateContextWithSeedData();
    }
}
