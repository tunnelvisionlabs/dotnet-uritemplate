namespace Rackspace.Net
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// This utility class determines the correct set of <see cref="RegexOptions"/> to specify when creating
    /// <see cref="Regex"/> instances at runtime within this library.
    /// </summary>
    internal static class InternalRegexOptions
    {
#if !PORTABLE
        /// <summary>
        /// The default <see cref="RegexOptions"/> for non-Portable Class Library builds.
        /// </summary>
        internal const RegexOptions Default = RegexOptions.Compiled | RegexOptions.CultureInvariant;
#else
        /// <summary>
        /// The default <see cref="RegexOptions"/> for Portable Class Library builds.
        /// </summary>
        internal const RegexOptions Default = RegexOptions.CultureInvariant;
#endif
    }
}
