namespace App.Services.Database.Models;

public partial class Message
{
    public ulong Id { get; set; }

    public ulong AuthorId { get; set; }

    public DateTime Date { get; set; }

    public DateTime? EditDate { get; set; }

    public string Content { get; set; } = null!;

    public ulong ChannelId { get; set; }

    public DateTime BackupDate { get; set; }

    public virtual Author Author { get; set; } = null!;

    public virtual BackupRegister BackupDateNavigation { get; set; } = null!;

    public virtual ICollection<BackupRegister> BackupRegisterEndMessages { get; set; } = new List<BackupRegister>();

    public virtual ICollection<BackupRegister> BackupRegisterStartMessages { get; set; } = new List<BackupRegister>();

    public virtual Channel Channel { get; set; } = null!;
}
