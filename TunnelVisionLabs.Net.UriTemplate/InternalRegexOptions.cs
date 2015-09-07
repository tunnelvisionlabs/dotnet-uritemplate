// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace TunnelVisionLabs.Net
{
    using System.Text.RegularExpressions;

#if PORTABLE
    using System;
#endif

    /// <summary>
    /// This utility class determines the correct set of <see cref="RegexOptions"/> to specify when creating
    /// <see cref="Regex"/> instances at runtime within this library.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    internal static class InternalRegexOptions
    {
        private static readonly RegexOptions RegexCompiledOption;

        static InternalRegexOptions()
        {
#if PORTABLE
            if (!Enum.TryParse("Compiled", out RegexCompiledOption))
                RegexCompiledOption = RegexOptions.None;
#else
            RegexCompiledOption = RegexOptions.Compiled;
#endif
        }

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
        /// <value>
        /// The default <see cref="RegexOptions"/> to use when creating new <see cref="Regex"/> instances.
        /// </value>
        public static RegexOptions Default
        {
            get
            {
                return RegexOptions.CultureInvariant | RegexCompiledOption;
            }
        }
    }
}
