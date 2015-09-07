// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace TunnelVisionLabs.Net
{
    /// <summary>
    /// Represents the URI Template part type described by a particular instance of <see cref="UriTemplatePart"/>.
    /// </summary>
    /// <preliminary/>
    internal enum UriTemplatePartType
    {
        /// <summary>
        /// A segment of literal text to include in the final URI. These parts are not part of an expression in the raw template.
        /// </summary>
        Literal,

        /// <summary>
        /// A simple string expansion with one or more variables. An example of the syntax for this part is <c>{x,y}</c>.
        /// </summary>
        SimpleStringExpansion,

        /// <summary>
        /// A reserved string expansion with one or more variables. An example of the syntax for this part is <c>{+x,y}</c>.
        /// </summary>
        ReservedStringExpansion,

        /// <summary>
        /// A fragment string expansion with one or more variables. An example of the syntax for this part is <c>{#x,y}</c>.
        /// </summary>
        FragmentExpansion,

        /// <summary>
        /// A dot-prefixed label expansion with one or more variables. An example of the syntax for this part is <c>{.x,y}</c>.
        /// </summary>
        LabelExpansion,

        /// <summary>
        /// One or more path segments formed from variables. An example of the syntax for this part is <c>{/x,y}</c>.
        /// </summary>
        PathSegments,

        /// <summary>
        /// One or more semicolon-prefixed path-style parameters. An example of the syntax for this part is <c>{;x,y}</c>.
        /// </summary>
        PathParameters,

        /// <summary>
        /// An ampersand-separated form-style query with one or more variables. An example of the syntax for this part is <c>{?x,y}</c>.
        /// </summary>
        Query,

        /// <summary>
        /// An ampersand-separated form-style query continuation with one or more variables. An example of the syntax for this part is <c>{&amp;x,y}</c>.
        /// </summary>
        QueryContinuation,
    }
}
