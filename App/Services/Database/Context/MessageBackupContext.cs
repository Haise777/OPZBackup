using Bot.Services.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Bot.Services.Database.Context;

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
            //.LogTo(ConsoleLogger.DBContextLogger)
            //.EnableSensitiveDataLogging()
            ;
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

            entity.HasIndex(e => e.AuthorId, "author");

            entity.HasIndex(e => e.ChannelId, "channel_id");

            entity.HasIndex(e => e.EndMessageId, "oldest_message");

            entity.HasIndex(e => e.StartMessageId, "youngest_message");

            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("date");
            entity.Property(e => e.AuthorId).HasColumnName("author_id");
            entity.Property(e => e.ChannelId).HasColumnName("channel_id");
            entity.Property(e => e.EndMessageId).HasColumnName("end_message_id");
            entity.Property(e => e.StartMessageId).HasColumnName("start_message_id");

            entity.HasOne(d => d.Author).WithMany(p => p.BackupRegisters)
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("backup_registers_ibfk_1");

            entity.HasOne(d => d.Channel).WithMany(p => p.BackupRegisters)
                .HasForeignKey(d => d.ChannelId)
                .HasConstraintName("backup_registers_ibfk_4");

            entity.HasOne(d => d.EndMessage).WithMany(p => p.BackupRegisterEndMessages)
                .HasForeignKey(d => d.EndMessageId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("backup_registers_ibfk_3");

            entity.HasOne(d => d.StartMessage).WithMany(p => p.BackupRegisterStartMessages)
                .HasForeignKey(d => d.StartMessageId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("backup_registers_ibfk_2");
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

            entity.HasIndex(e => e.AuthorId, "author");

            entity.HasIndex(e => e.BackupDate, "backup_date");

            entity.HasIndex(e => e.ChannelId, "channel_id");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AuthorId).HasColumnName("author_id");
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

            entity.HasOne(d => d.Author).WithMany(p => p.Messages)
                .HasForeignKey(d => d.AuthorId)
                .HasConstraintName("messages_ibfk_1");

            entity.HasOne(d => d.BackupDateNavigation).WithMany(p => p.Messages)
                .HasForeignKey(d => d.BackupDate)
                .HasConstraintName("messages_ibfk_3");

            entity.HasOne(d => d.Channel).WithMany(p => p.Messages)
                .HasForeignKey(d => d.ChannelId)
                .HasConstraintName("messages_ibfk_2");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
