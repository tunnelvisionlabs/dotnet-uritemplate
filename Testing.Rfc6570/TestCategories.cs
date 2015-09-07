// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Testing.Rfc6570
{
    internal static class TestCategories
    {
        /// <summary>
        /// The test contains a Level 1 URI Template.
        /// </summary>
        public const string Level1 = "Level 1";

        /// <summary>
        /// The test contains a Level 2 URI Template.
        /// </summary>
        public const string Level2 = "Level 2";

        /// <summary>
        /// The test contains a Level 3 URI Template.
        /// </summary>
        public const string Level3 = "Level 3";

        /// <summary>
        /// The test contains a Level 4 URI Template.
        /// </summary>
        public const string Level4 = "Level 4";

        /// <summary>
        /// The test contains an invalid URI Template or parameter value.
        /// </summary>
        public const string InvalidTemplates = "Invalid Templates";

        /// <summary>
        /// The test contains an extended URI Template test.
        /// </summary>
        public const string Extended = "Extended";

        /// <summary>
        /// The test contains a simple string expansion: <c>{var}</c>.
        /// </summary>
        public const string SimpleExpansion = "Expansions: Simple";

        /// <summary>
        /// The test contains a reserved expansion: <c>{+var}</c>.
        /// </summary>
        public const string ReservedExpansion = "Expansions: Reserved";

        /// <summary>
        /// The test contains a fragment expansion: <c>{#var}</c>.
        /// </summary>
        public const string FragmentExpansion = "Expansions: Fragment";

        /// <summary>
        /// The test contains a label expansion with dot-prefix: <c>{.var}</c>.
        /// </summary>
        public const string LabelExpansion = "Expansions: Label";

        /// <summary>
        /// The test contains a path segment expansion: <c>{/var}</c>.
        /// </summary>
        public const string PathSegmentExpansion = "Expansions: Path Segment";

        /// <summary>
        /// The test contains a path-style parameter expansion: <c>{;var}</c>.
        /// </summary>
        public const string PathParameterExpansion = "Expansions: Path Parameter";

        /// <summary>
        /// The test contains a form-style query expansion: <c>{?var}</c>.
        /// </summary>
        public const string QueryExpansion = "Expansions: Query";

        /// <summary>
        /// The test contains a form-style query continuation expansion: <c>{&amp;var}</c>.
        /// </summary>
        public const string QueryContinuationExpansion = "Expansions: Query Continuation";
    }
}
