using App.Services.Models;
using Microsoft.EntityFrameworkCore;

namespace App.Services.Context;

public partial class MessageBackupContext : DbContext
{
    public MessageBackupContext()
    {
    }

    public MessageBackupContext(DbContextOptions<MessageBackupContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Author> Authors { get; set; }

    public virtual DbSet<BackupRegister> BackupRegisters { get; set; }

    public virtual DbSet<Channel> Channels { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySql(File.ReadAllText(@"E:\archives\privateapplocals\sc.txt"), ServerVersion.Parse("8.0.34-mysql"))
            .LogTo(ConsoleLogger.DBContext)
            //TODO WARNING: Remove sensitive logging when not in development testing
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("authors");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Username)
                .HasMaxLength(30)
                .HasColumnName("username");
        });

        modelBuilder.Entity<BackupRegister>(entity =>
        {
            entity.HasKey(e => e.Date).HasName("PRIMARY");

            entity.ToTable("backup_registers");

            entity.HasIndex(e => e.Author, "author");

            entity.HasIndex(e => e.ChannelId, "channel");

            entity.HasIndex(e => e.OldestMessage, "oldest_message");

            entity.HasIndex(e => e.YoungestMessage, "youngest_message");

            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("date");
            entity.Property(e => e.Author).HasColumnName("author");
            entity.Property(e => e.ChannelId).HasColumnName("channel_id");
            entity.Property(e => e.OldestMessage).HasColumnName("oldest_message");
            entity.Property(e => e.YoungestMessage).HasColumnName("youngest_message");

            entity.HasOne(d => d.AuthorNavigation).WithMany(p => p.BackupRegisters)
                .HasForeignKey(d => d.Author)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("backup_registers_ibfk_1");

            entity.HasOne(d => d.Channel).WithMany(p => p.BackupRegisters)
                .HasForeignKey(d => d.ChannelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("backup_registers_ibfk_2");

            entity.HasOne(d => d.OldestMessageNavigation).WithMany(p => p.BackupRegisterOldestMessageNavigations)
                .HasForeignKey(d => d.OldestMessage)
                .HasConstraintName("backup_registers_ibfk_4");

            entity.HasOne(d => d.YoungestMessageNavigation).WithMany(p => p.BackupRegisterYoungestMessageNavigations)
                .HasForeignKey(d => d.YoungestMessage)
                .HasConstraintName("backup_registers_ibfk_3");
        });

        modelBuilder.Entity<Channel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("channels");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(30)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("messages");

            entity.HasIndex(e => e.Author, "author");

            entity.HasIndex(e => e.BackupDate, "backup_date");

            entity.HasIndex(e => e.ChannelId, "channel_id");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Author).HasColumnName("author");
            entity.Property(e => e.BackupDate)
                .HasColumnType("datetime")
                .HasColumnName("backup_date");
            entity.Property(e => e.ChannelId).HasColumnName("channel_id");
            entity.Property(e => e.Content)
                .HasMaxLength(2000)
                .HasColumnName("content");
            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("date");
            entity.Property(e => e.EditDate)
                .HasColumnType("datetime")
                .HasColumnName("edit_date");

            entity.HasOne(d => d.AuthorNavigation).WithMany(p => p.Messages)
                .HasForeignKey(d => d.Author)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("messages_ibfk_2");

            entity.HasOne(d => d.BackupDateNavigation).WithMany(p => p.Messages)
                .HasForeignKey(d => d.BackupDate)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("messages_ibfk_3");

            entity.HasOne(d => d.Channel).WithMany(p => p.Messages)
                .HasForeignKey(d => d.ChannelId)
                .HasConstraintName("messages_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
