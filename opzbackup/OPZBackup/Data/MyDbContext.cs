using Microsoft.EntityFrameworkCore;
using OPZBackup.Data.Models;

namespace OPZBackup.Data;

public partial class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
    {
    }

    public virtual DbSet<BackupRegistry> BackupRegistries { get; set; }

    public virtual DbSet<Channel> Channels { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<AttachmentFile> AttachmentFiles{ get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BackupRegistry>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("backup_registry");

            entity.HasIndex(e => e.ChannelId, "backup_registry_channels_id_fk");

            entity.HasIndex(e => e.AuthorId, "backup_registry_users_id_fk");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.AuthorId).HasColumnName("author_id");
            entity.Property(e => e.ChannelId).HasColumnName("channel_id");
            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("date");

            entity.HasOne(d => d.Author).WithMany(p => p.BackupRegistries)
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("backup_registry_users_id_fk");

            entity.HasOne(d => d.Channel).WithMany(p => p.BackupRegistries)
                .HasForeignKey(d => d.ChannelId)
                .HasConstraintName("backup_registry_channels_id_fk");
        });

        modelBuilder.Entity<Channel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("channels");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("messages");

            entity.HasIndex(e => e.BackupId, "messages_backup_registry_id_fk");

            entity.HasIndex(e => e.ChannelId, "messages_channels_id_fk");

            entity.HasIndex(e => e.AuthorId, "messages_users_id_fk");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AuthorId).HasColumnName("author_id");
            entity.Property(e => e.BackupId).HasColumnName("backup_id");
            entity.Property(e => e.ChannelId).HasColumnName("channel_id");
            entity.Property(e => e.Content)
                .HasMaxLength(5000)
                .HasColumnName("content");
            entity.Property(e => e.HasFile)
                .HasColumnName("has_file");
            entity.Property(e => e.SentDate)
                .HasColumnType("datetime")
                .HasColumnName("sent_date");

            entity.HasOne(d => d.Author).WithMany(p => p.Messages)
                .HasForeignKey(d => d.AuthorId)
                .HasConstraintName("messages_users_id_fk");

            entity.HasOne(d => d.Backup).WithMany(p => p.Messages)
                .HasForeignKey(d => d.BackupId)
                .HasConstraintName("messages_backup_registry_id_fk");

            entity.HasOne(d => d.Channel).WithMany(p => p.Messages)
                .HasForeignKey(d => d.ChannelId)
                .HasConstraintName("messages_channels_id_fk");
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
            entity.Property(e => e.IsBlackListed)
                .HasColumnName("is_blacklisted");
        });

        modelBuilder.Entity<AttachmentFile>(entity =>
        {

            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasIndex(e => e.MessageId, "attachment_message_id_fk");

            entity.ToTable("attachment_files");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Extension)
                .HasMaxLength(15)
                .HasColumnName("extension");
            entity.Property(e => e.Path)
                .HasMaxLength(255)
                .HasColumnName("path");
            entity.Property(e => e.ByteSize)
                .HasColumnName("byte_size");       
            entity.Property(e => e.MessageId)
                .HasColumnName("message_id");      

            entity.HasOne(e => e.Message).WithMany(e => e.Attachments)
                .HasForeignKey(e => e.MessageId)
                .HasConstraintName("attachment_message_id_fk");
        
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}