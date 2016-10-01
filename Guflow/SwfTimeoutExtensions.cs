namespace Guflow
{
    internal static class SwfTimeoutExtensions
    {
        public static string ToAwsFormat(this uint? timeout)
        {
            if(!timeout.HasValue)
                return null;
            return timeout.Value.ToString();
        }
    }
}