using System.Text.RegularExpressions;

namespace RestExceptions;

internal static partial class StringExtensions
{
    /// <summary>
    /// Converts a string to kebab-case.
    /// </summary>
    /// <param name="str">The string to convert.</param>
    /// <returns>A kebab-case version of the string.</returns>
    /// <example>
    /// "This is a test" becomes "this-is-a-test".
    /// </example>
    internal static string ToKebabCase(this string str)
    {
        return string.IsNullOrEmpty(str)
            ? str
            : NotWordRegex().Replace(str.ToLowerInvariant(), "-").Trim('-');
    }

    [GeneratedRegex(@"[^\w]+")]
    private static partial Regex NotWordRegex();
}
