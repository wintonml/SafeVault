using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SafeVault.Models.Scaffolded;

namespace SafeVault.Data;

public partial class SafeVaultContext : DbContext
{
    public SafeVaultContext(DbContextOptions<SafeVaultContext> options)
        : base(options)
    {
    }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email, "IX_Users_Email").IsUnique();

            entity.HasIndex(e => e.Username, "IX_Users_Username").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Email).HasColumnType("VARCHAR(100)");
            entity.Property(e => e.Password).HasColumnType("VARCHAR(100)");
            entity.Property(e => e.Role)
                .HasDefaultValue("User")
                .HasColumnType("VARCHAR(20)");
            entity.Property(e => e.Username).HasColumnType("VARCHAR(100)");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
