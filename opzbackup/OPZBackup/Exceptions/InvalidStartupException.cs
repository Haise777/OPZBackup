namespace OPZBackup.Exceptions;

public class InvalidStartupException : Exception
{
    public InvalidStartupException(string message) : base(message)
    {
    }

    public InvalidStartupException(string message, Exception innerException) : base(message, innerException)
    {
    }
}