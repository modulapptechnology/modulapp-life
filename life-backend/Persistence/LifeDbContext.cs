using Microsoft.EntityFrameworkCore;

namespace Persistence;

public class LifeDbContext(DbContextOptions<LifeDbContext> options) : DbContext(options)
{
}
