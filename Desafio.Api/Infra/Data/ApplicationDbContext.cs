using System.Reflection;
using Desafio.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Desafio.Api.Infra.Data;

public sealed class ApplicationDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    
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