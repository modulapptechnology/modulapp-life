using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Tests.Support;

public static class TestLifeDbContext
{
    public static LifeDbContext Create(string connectionString)
    {
        var options = new DbContextOptionsBuilder<LifeDbContext>().UseSqlServer(connectionString).Options;

        return new LifeDbContext(options);
    }
}
