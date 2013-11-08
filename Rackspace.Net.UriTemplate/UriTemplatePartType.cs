namespace Rackspace.Net
{
    internal enum UriTemplatePartType
    {
        /// <summary>
        /// A segment of literal text to include in the final URI.
        /// </summary>
        Literal,

        /// <summary>
        /// A simple string expansion with one or more variables.
        /// </summary>
        SimpleStringExpansion,

        /// <summary>
        /// A reserved string expansion with one or more variables.
        /// </summary>
        ReservedStringExpansion,

        /// <summary>
        /// A fragment string expansion with one or more variables.
        /// </summary>
        FragmentExpansion,

        /// <summary>
        /// A dot-prefixed label expansion with one or more variables.
        /// </summary>
        LabelExpansion,

        /// <summary>
        /// One or more path segments formed from variables.
        /// </summary>
        PathSegments,

        /// <summary>
        /// One or more semicolon-prefixed path-style parameters.
        /// </summary>
        PathParameters,

        /// <summary>
        /// An ampersand-separated form-style query with one or more variables.
        /// </summary>
        Query,

        /// <summary>
        /// An ampersand-separated form-style query continuation with one or more variables.
        /// </summary>
        QueryContinuation,
    }
}
