using System;
using System.Globalization;


namespace MyExtensions
{
    public static class MyExtensions
    {
        private static readonly string[] SupportedFormats = new[]
        {
        "dddd, MMMM dd, yyyy hh:mm:ss tt", // Tuesday, November 18, 2025 11:30:01 PM
        "ddd MMM dd HH:mm:ss yyyy",        // Sun Dec 07 13:14:01 2025
        "ddd MMM d HH:mm:ss yyyy"         // Sun Dec 7 13:14:01 2025 (non-zero-padded day)
        };

        /// <summary>
        /// Attempts to parse a string into DateTime using multiple known formats.
        /// </summary>
        public static DateTime ParseFlexible(this string input)
        {
            if (DateTime.TryParseExact(input,
                SupportedFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime result))
            {
                return result;
            }

            // fallback: try normal Parse (handles many common formats)
            return DateTime.Parse(input, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Safe version that returns null if parsing fails.
        /// </summary>
        public static DateTime? TryParseFlexible(this string input)
        {
            if (DateTime.TryParseExact(input,
                SupportedFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime result))
            {
                return result;
            }

            if (DateTime.TryParse(input, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out result))
            {
                return result;
            }

            return null;
        }
    }

}
