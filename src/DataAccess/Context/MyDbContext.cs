using System;
using System.Collections.Generic;
using Data.Contracts.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data.Contracts.Context;

public partial class MyDbContext : DbContext
{
    public MyDbContext()
    {
    }

    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BackupRegistry> BackupRegistries { get; set; }

    public virtual DbSet<Channel> Channels { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<User> Users { get; set; }
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<BackupRegistry>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("backup_registry");

            entity.HasIndex(e => e.AuthorId, "fk_author");

            entity.HasIndex(e => e.ChannelId, "fk_channel");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AuthorId).HasColumnName("author_id");
            entity.Property(e => e.ChannelId).HasColumnName("channel_id");
            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("date");

            entity.HasOne(d => d.Author).WithMany(p => p.BackupRegistries)
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("backup_registry_ibfk_1");

            entity.HasOne(d => d.Channel).WithMany(p => p.BackupRegistries)
                .HasForeignKey(d => d.ChannelId)
                .HasConstraintName("backup_registry_ibfk_2");
        });

        modelBuilder.Entity<Channel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("channels");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("messages");

            entity.HasIndex(e => e.AuthorId, "fk_author");

            entity.HasIndex(e => e.BackupId, "fk_backup");

            entity.HasIndex(e => e.ChannelId, "fk_channel");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AuthorId).HasColumnName("author_id");
            entity.Property(e => e.BackupId).HasColumnName("backup_id");
            entity.Property(e => e.ChannelId).HasColumnName("channel_id");
            entity.Property(e => e.Content)
                .HasMaxLength(5000)
                .HasColumnName("content");
            entity.Property(e => e.SentDate)
                .HasColumnType("datetime")
                .HasColumnName("sent_date");

            entity.HasOne(d => d.Author).WithMany(p => p.Messages)
                .HasForeignKey(d => d.AuthorId)
                .HasConstraintName("messages_ibfk_1");

            entity.HasOne(d => d.Backup).WithMany(p => p.Messages)
                .HasForeignKey(d => d.BackupId)
                .HasConstraintName("messages_ibfk_3");

            entity.HasOne(d => d.Channel).WithMany(p => p.Messages)
                .HasForeignKey(d => d.ChannelId)
                .HasConstraintName("messages_ibfk_2");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("users");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Username)
                .HasMaxLength(35)
                .HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}