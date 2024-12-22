using System.Text;

namespace OPZBackup.Logger;

public static class OutputTemplate
{
    public static string DefaultTemplate(string propertyName)
    {
        var builder = new StringBuilder();

        builder.Append("[{Timestamp:HH:mm:ss} {Level:u3} ");
        builder.Append($"{{{propertyName}}}] ");
        builder.Append("{Message}{NewLine}{Exception}");

        return builder.ToString();
    }

    public static string DefaultTemplateSplitted(string propertyName)
    {
        var builder = new StringBuilder();

        builder.Append("[{Timestamp:HH:mm:ss} {Level:u3} ");
        builder.Append($"{{{propertyName}}}] ");
        builder.Append("{NewLine} - {Message}{NewLine}{Exception}");

        return builder.ToString();
    }
}