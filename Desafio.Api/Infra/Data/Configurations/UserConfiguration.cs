using Desafio.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Desafio.Api.Infra.Data.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.Property(m => m.Name)
            .HasMaxLength(250)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(m => m.Email)
            .HasMaxLength(250)
            .IsUnicode(false)
            .IsRequired();

        builder.HasIndex(m => m.Email)
            .IsUnique();
    }
}