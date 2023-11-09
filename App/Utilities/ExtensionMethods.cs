namespace App.Utilities
{
    internal static class ExtensionMethods
    {
        public static DateTime WithoutMilliseconds(this DateTime dt)
            => new(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, 0, dt.Kind);
    }
}
