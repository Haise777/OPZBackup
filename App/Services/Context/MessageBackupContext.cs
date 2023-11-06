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

    public virtual DbSet<Channel> Channels { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySql(File.ReadAllText(@"E:\archives\privateapplocals\sc.txt"), ServerVersion.Parse("8.0.34-mysql"))
            .LogTo(DbLogger.LogContext)
            //TODO WARNING: Remove sensitive logging when not in development testing
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Channel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("channels");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Category)
                .HasMaxLength(30)
                .HasColumnName("category");
            entity.Property(e => e.Name)
                .HasMaxLength(30)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("messages");

            entity.HasIndex(e => e.ChannelId, "channel_id");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Author)
                .HasMaxLength(20)
                .HasColumnName("author");
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

            entity.HasOne(d => d.Channel).WithMany(p => p.Messages)
                .HasForeignKey(d => d.ChannelId)
                .HasConstraintName("messages_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
