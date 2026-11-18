using Npgsql;
using Testcontainers.PostgreSql;
[TestClass]
public sealed class DatabaseTests
{
    private PostgreSqlContainer database = null!;
    private NpgsqlConnection connection = null!;
    [TestInitialize]
    public async Task Start_an_owned_database_instance()
    {
        database = new PostgreSqlBuilder("postgres:17-alpine")
            .WithDatabase("course_tests").WithUsername("course").WithPassword("course-local").Build();
        await database.StartAsync();
        connection = new NpgsqlConnection(database.GetConnectionString());
        await connection.OpenAsync();
        await new NpgsqlCommand(CourseSchema.Create, connection).ExecuteNonQueryAsync();
    }
    [TestCleanup]
    public async Task Dispose_the_owned_database_instance()
    {
        if (connection is not null) await connection.DisposeAsync();
        if (database is not null) await database.DisposeAsync();
    }
    [TestMethod]
    public async Task PostgreSQL_enforces_the_primary_key()
    {
        await new NpgsqlCommand("INSERT INTO courses VALUES(11,'Database testing')",connection).ExecuteNonQueryAsync();
        var exception = await Assert.ThrowsExactlyAsync<PostgresException>(async () =>
            await new NpgsqlCommand("INSERT INTO courses VALUES(11,'Duplicate')",connection).ExecuteNonQueryAsync());
        Assert.AreEqual(PostgresErrorCodes.UniqueViolation, exception.SqlState);
    }
    [TestMethod]
    public async Task Rollback_removes_uncommitted_work()
    {
        await using var transaction = await connection.BeginTransactionAsync();
        await new NpgsqlCommand("INSERT INTO courses VALUES(12,'Rolled back')",connection,transaction).ExecuteNonQueryAsync();
        await transaction.RollbackAsync();
        var count = (long)(await new NpgsqlCommand("SELECT COUNT(*) FROM courses",connection).ExecuteScalarAsync())!;
        Assert.AreEqual(0L, count);
    }
    [TestMethod]
    public async Task Each_test_starts_with_an_empty_database()
    {
        var count = (long)(await new NpgsqlCommand("SELECT COUNT(*) FROM courses",connection).ExecuteScalarAsync())!;
        Assert.AreEqual(0L, count);
        using var command = new NpgsqlCommand("SELECT version()",connection);
        StringAssert.Contains((string)(await command.ExecuteScalarAsync())!, "PostgreSQL 17");
    }
}
