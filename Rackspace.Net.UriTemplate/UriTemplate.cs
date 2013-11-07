namespace Rackspace.Net
{
    /// <summary>
    /// A class that represents an RFC 6570 URI Template.
    /// </summary>
    public class UriTemplate
    {
        /// <summary>
        /// A regular expression pattern for the RFC 6570 <c>pct-encoded</c> rule.
        /// </summary>
        internal const string PctEncodedPattern = @"(?:%[a-fA-F0-9]{2})";

        /// <summary>
        /// A regular expression pattern for the RFC 6570 <c>varchar</c> rule.
        /// </summary>
        internal const string VarCharPattern = @"(?:[a-zA-Z0-9_]|" + PctEncodedPattern + @")";

        /// <summary>
        /// A regular expression pattern for the RFC 6570 <c>varname</c> rule.
        /// </summary>
        internal const string VarNamePattern = @"(?:" + VarCharPattern + @"(?:\.?" + VarCharPattern + @")*)";

        /// <summary>
        /// A regular expression pattern for the RFC 6570 <c>varspec</c> rule.
        /// </summary>
        internal const string VarSpecPattern = @"(?:" + VarNamePattern + @"(?:\:[1-9][0-9]{0,3}|\*)?" + @")";

        /// <summary>
        /// A regular expression pattern for the RFC 6570 <c>variable-list</c> rule.
        /// </summary>
        internal const string VariableListPattern = @"(?:" + VarSpecPattern + @"(?:," + VarSpecPattern + @")*)";
    }
}
