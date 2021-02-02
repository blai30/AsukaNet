namespace System
{
    /// <summary>
    /// Extension methods for strings.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Removes characters from the string past a given maxLength.
        /// </summary>
        /// <param name="value">String to truncate.</param>
        /// <param name="maxLength">Max length of characters to preserve.</param>
        /// <param name="ellipses">Replaces the last 3 characters after truncating with "..." if true.</param>
        /// <returns>Truncated string.</returns>
        public static string Truncate(this string value, int maxLength, bool ellipses = false)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return ellipses ?
                value.Length <= maxLength ?
                    value :
                    value.Substring(0, maxLength - 3) + "..." :
                value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}
