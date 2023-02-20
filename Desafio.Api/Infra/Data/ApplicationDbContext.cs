using System.Reflection;
using Desafio.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Desafio.Api.Infra.Data;

public class ApplicationDbContext : DbContext
{
    public virtual DbSet<User> Users => Set<User>();

    protected ApplicationDbContext() { }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(builder);
    }
}