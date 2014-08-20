namespace Rackspace.Net
{
    using System.Text.RegularExpressions;

#if PORTABLE
    using System;
#endif

    /// <summary>
    /// This utility class determines the correct set of <see cref="RegexOptions"/> to specify when creating
    /// <see cref="Regex"/> instances at runtime within this library.
    /// </summary>
    internal static class InternalRegexOptions
    {
#if PORTABLE
        /// <summary>
        /// This is the backing field for the <see cref="Default"/> property.
        /// </summary>
        private static RegexOptions? _default;

        /// <see href="http://referencesource.microsoft.com/#System/regex/system/text/regularexpressions/RegexOptions.cs#40">RegexOptions.Compiled</see>
        private const RegexOptions RegexCompiledOption = (RegexOptions)0x0008;

        /// <summary>
        /// These options are preferred for any runtime which supports the combination.
        /// </summary>
        private const RegexOptions PreferredOptions = RegexOptions.CultureInvariant | RegexCompiledOption;

        /// <summary>
        /// These options are not preferred, but they are a safe fall-back for any runtime which does not support
        /// <see cref="F:System.Text.RegularExpressions.RegexOptions.Compiled"/>.
        /// </summary>
        private const RegexOptions FallbackOptions = RegexOptions.CultureInvariant;
#endif

        /// <summary>
        /// Gets the default <see cref="RegexOptions"/> to use when creating new <see cref="Regex"/> instances.
        /// </summary>
        /// <remarks>
        /// <para>When targeting the Portable Class Library, the
        /// <see cref="F:System.Text.RegularExpressions.RegexOptions.Compiled"/> option is not always available. It
        /// does, however, have a consistent value when it is supported. The default options returned by this method
        /// include <see cref="RegexOptions.CultureInvariant"/> and, when supported,
        /// <see cref="F:System.Text.RegularExpressions.RegexOptions.Compiled"/>.</para>
        /// </remarks>
        public static RegexOptions Default
        {
            get
            {
#if !PORTABLE
                return RegexOptions.CultureInvariant | RegexOptions.Compiled;
#else
                RegexOptions? defaultOptions = _default;
                if (!defaultOptions.HasValue)
                {
                    defaultOptions = GetDefaultOptions();
                    _default = defaultOptions;
                }

                return defaultOptions.Value;
#endif
            }
        }

#if PORTABLE
        /// <summary>
        /// This method tests the current runtime to determine which set of default <see cref="RegexOptions"/> to use.
        /// </summary>
        private static RegexOptions GetDefaultOptions()
        {
            RegexOptions options = RegexOptions.CultureInvariant | RegexCompiledOption;
            try
            {
                Regex testExpression = new Regex("^x$", options);
                return testExpression.IsMatch("x") ? options : FallbackOptions;
            }
            catch (ArgumentException)
            {
                return FallbackOptions;
            }
            catch (NotSupportedException)
            {
                return FallbackOptions;
            }
        }
#endif
    }
}
