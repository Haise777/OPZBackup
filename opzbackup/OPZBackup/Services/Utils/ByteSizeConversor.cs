namespace OPZBackup.Services.Utils;

public static class ByteSizeConversor
{
    public static string ToFormattedString(this ulong byteSize)
    {
        string[] scale = ["B", "KB", "MB", "GB", "TB", "PB"];
        var order = 0;
        float formattedSize = byteSize;

        while (formattedSize >= 1024 && order < scale.Length - 1)
        {
            order++;
            formattedSize /= 1024;
        }

        return $"{formattedSize:0.##} {scale[order]}";
    }
}