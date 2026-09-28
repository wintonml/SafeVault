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
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Email).HasColumnType("VARCHAR(100)");
            entity.Property(e => e.Username).HasColumnType("VARCHAR(100)");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
