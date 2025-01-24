namespace OPZBackup.Data.Models;

public class AttachmentFile
{
    public int Id { get; set; }
    public string Name { get; set;}
    public string Extension { get; set;}
    public string Path { get; set;}
    public ulong ByteSize { get; set;}

    public ulong MessageId { get; set; }
    public virtual Message? Message { get; set; }

}