using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PosnaiSQLauncher.Entities;

namespace PosnaiSQLauncher.Context;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Database> Databases { get; set; }

    public virtual DbSet<Location> Locations { get; set; }

    public virtual DbSet<Option> Options { get; set; }

    public virtual DbSet<Query> Queries { get; set; }

    public virtual DbSet<ShowOption> ShowOptions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "databin.json");
            string json = File.ReadAllText(configPath);
            string? connectionString = JObject.Parse(json)["SQLString"]?.ToString();

            optionsBuilder.UseSqlServer(connectionString);
        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Database>(entity =>
        {
            entity.HasKey(e => e.IdDatabase).HasName("PK__Database__A75416E85109A4FA");

            entity.ToTable("Database");
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.IdLocation).HasName("PK__Location__FB5FABA9CF952265");

            entity.ToTable("Location");
        });

        modelBuilder.Entity<Option>(entity =>
        {
            entity.HasKey(e => e.IdOption).HasName("PK__Option__C118A0F1AB48EB57");

            entity.ToTable("Option");

            entity.Property(e => e.TimeLimit).HasDefaultValue(300);

            entity.HasOne(d => d.IdLocationNavigation).WithMany(p => p.Options)
                .HasForeignKey(d => d.IdLocation)
                .HasConstraintName("FK_Option_Location");

            entity.HasOne(d => d.IdQueryNavigation).WithMany(p => p.Options)
                .HasForeignKey(d => d.IdQuery)
                .HasConstraintName("FK_Option_Query");
        });

        modelBuilder.Entity<Query>(entity =>
        {
            entity.HasKey(e => e.IdQuery).HasName("PK__Query__CDB81D9DB1814ACA");

            entity.ToTable("Query");

            entity.HasOne(d => d.IdDatabaseNavigation).WithMany(p => p.Queries)
                .HasForeignKey(d => d.IdDatabase)
                .HasConstraintName("FK_Query_Database");
        });

        modelBuilder.Entity<ShowOption>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("ShowOption");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
