using Life.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Life.Infrastructure.Tests.Support;

public static class TestLifeDbContext
{
    public static LifeDbContext Create(string connectionString)
    {
        var options = new DbContextOptionsBuilder<LifeDbContext>().UseSqlServer(connectionString).Options;

        return new LifeDbContext(options);
    }
}
