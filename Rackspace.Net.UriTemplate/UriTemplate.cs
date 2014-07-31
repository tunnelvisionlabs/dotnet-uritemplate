// Copyright (c) Rackspace, US Inc. All Rights Reserved. Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Rackspace.Net
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// A class that represents an RFC 6570 URI Template.
    /// </summary>
    /// <threadsafety static="true" instance="true"/>
    /// <preliminary/>
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

        /// <summary>
        /// A regular expression pattern for the RFC 6570 <c>op-level2</c> rule.
        /// </summary>
        internal const string OperatorLevel2Pattern = @"[+#]";

        /// <summary>
        /// A regular expression pattern for the RFC 6570 <c>op-level3</c> rule.
        /// </summary>
        internal const string OperatorLevel3Pattern = @"[./;?&]";

        /// <summary>
        /// A regular expression pattern for the RFC 6570 <c>op-reserve</c> rule.
        /// </summary>
        internal const string OperatorReservePattern = @"[=,!@|]";

        /// <summary>
        /// A regular expression pattern for the RFC 6570 <c>operator</c> rule.
        /// </summary>
        internal const string OperatorPattern = "(?:" + OperatorLevel2Pattern + "|" + OperatorLevel3Pattern + "|" + OperatorReservePattern + ")";

        /// <summary>
        /// A regular expression pattern for the RFC 6570 <c>expression</c> rule.
        /// </summary>
        internal const string ExpressionPattern = @"{" + OperatorPattern + @"?" + VariableListPattern + @"}";

#if !PORTABLE
        /// <summary>
        /// The default <see cref="RegexOptions"/> for non-Portable Class Library builds.
        /// </summary>
        internal const RegexOptions DefaultRegexOptions = RegexOptions.Compiled | RegexOptions.CultureInvariant;
#else
        /// <summary>
        /// The default <see cref="RegexOptions"/> for Portable Class Library builds.
        /// </summary>
        internal const RegexOptions DefaultRegexOptions = RegexOptions.CultureInvariant;
#endif

        /// <summary>
        /// A regular expression which matches a single <c>expression</c> within a URI Template.
        /// </summary>
        /// <remarks>
        /// This regular expression has the following named captures.
        /// <list type="table">
        /// <listheader>
        /// <term>Name</term>
        /// <term>Meaning</term>
        /// </listheader>
        /// <item>
        /// <description><c>Operator</c></description>
        /// <description>The (optional) <c>operator</c> portion of the <c>expression</c>, described by the <see cref="OperatorPattern"/> pattern.</description>
        /// </item>
        /// <item>
        /// <description><c>VariableList</c></description>
        /// <description>The <c>variable-list</c> portion of the <c>expression</c>, described by the <see cref="VariableListPattern"/> pattern.</description>
        /// </item>
        /// </list>
        /// </remarks>
        private static readonly Regex ExpressionExpression =
            new Regex(@"{(?<Operator>" + OperatorPattern + @")?(?<VariableList>" + VariableListPattern + @")}", DefaultRegexOptions);

        /// <summary>
        /// This is the backing field for the <see cref="Template"/> property.
        /// </summary>
        private readonly string _template;

        /// <summary>
        /// An array of <see cref="UriTemplatePart"/> instances representing the decomposed URI Template.
        /// Each part is responsible for rendering its own value.
        /// </summary>
        private readonly UriTemplatePart[] _parts;

        /// <summary>
        /// Initializes a new instance of the <see cref="UriTemplate"/> class
        /// using the specified template.
        /// </summary>
        /// <param name="template">The URI template.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="template"/> is <see langword="null"/>.</exception>
        /// <exception cref="NotSupportedException">If <paramref name="template"/> contains a variable expansion which uses an operator which is reserved for future extensions, but not defined for Level 4 URI Templates.</exception>
        /// <exception cref="FormatException">If <paramref name="template"/> is not a valid <c>URI-Template</c> according to RFC 6570.</exception>
        public UriTemplate(string template)
        {
            if (template == null)
                throw new ArgumentNullException("template");

            _template = template;
            _parts = ParseTemplate(template);
        }

        /// <summary>
        /// Gets the original template text the <see cref="UriTemplate"/> was constructed from.
        /// </summary>
        public string Template
        {
            get
            {
                return _template;
            }
        }

        private static UriTemplatePart[] ParseTemplate(string template)
        {
            List<UriTemplatePart> parts = new List<UriTemplatePart>();
            int previousEnd = 0;
            foreach (Match match in ExpressionExpression.Matches(template))
            {
                if (match.Index > previousEnd)
                    parts.Add(new UriTemplatePartLiteral(template.Substring(previousEnd, match.Index - previousEnd)));

                UriTemplatePartType type = UriTemplatePartType.SimpleStringExpansion;
                Group op = match.Groups["Operator"];
                if (op.Success && op.Length > 0)
                {
                    switch (op.Value)
                    {
                    case "+":
                        type = UriTemplatePartType.ReservedStringExpansion;
                        break;

                    case "#":
                        type = UriTemplatePartType.FragmentExpansion;
                        break;

                    case ".":
                        type = UriTemplatePartType.LabelExpansion;
                        break;

                    case "/":
                        type = UriTemplatePartType.PathSegments;
                        break;

                    case ";":
                        type = UriTemplatePartType.PathParameters;
                        break;

                    case "?":
                        type = UriTemplatePartType.Query;
                        break;

                    case "&":
                        type = UriTemplatePartType.QueryContinuation;
                        break;

                    case "=":
                    case ",":
                    case "!":
                    case "@":
                    case "|":
                        throw new NotSupportedException(string.Format("Operator is reserved for future expansion: {0}", op.Value));

                    default:
                        throw new InvalidOperationException("Unreachable");
                    }
                }

                Group variableList = match.Groups["VariableList"];
                IEnumerable<VariableReference> variables;
                if (variableList.Success)
                {
                    string[] specs = variableList.Value.Split(',');
                    variables = specs.Select(VariableReference.Parse);
                }
                else
                {
                    variables = Enumerable.Empty<VariableReference>();
                }

                UriTemplatePart part;
                switch (type)
                {
                case UriTemplatePartType.SimpleStringExpansion:
                    part = new UriTemplatePartSimpleExpansion(variables, true);
                    break;

                case UriTemplatePartType.ReservedStringExpansion:
                    part = new UriTemplatePartSimpleExpansion(variables, false);
                    break;

                case UriTemplatePartType.FragmentExpansion:
                    part = new UriTemplatePartFragmentExpansion(variables);
                    break;

                case UriTemplatePartType.LabelExpansion:
                    part = new UriTemplatePartLabelExpansion(variables);
                    break;

                case UriTemplatePartType.PathSegments:
                    part = new UriTemplatePartPathSegmentExpansion(variables);
                    break;

                case UriTemplatePartType.PathParameters:
                    part = new UriTemplatePartPathParametersExpansion(variables);
                    break;

                case UriTemplatePartType.Query:
                    part = new UriTemplatePartQueryExpansion(variables, false);
                    break;

                case UriTemplatePartType.QueryContinuation:
                    part = new UriTemplatePartQueryExpansion(variables, true);
                    break;

                case UriTemplatePartType.Literal:
                default:
                    throw new InvalidOperationException("Unreachable");
                }

                parts.Add(part);
                previousEnd = match.Index + match.Length;
            }

            if (previousEnd < template.Length)
                parts.Add(new UriTemplatePartLiteral(template.Substring(previousEnd)));

            return parts.ToArray();
        }

        /// <summary>
        /// Creates a new URI from the template and the collection of parameters.
        /// </summary>
        /// <param name="parameters">The parameter values.</param>
        /// <returns>A <see cref="Uri"/> object representing the expanded template.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="parameters"/> is <see langword="null"/>.</exception>
        public Uri BindByName(IDictionary<string, string> parameters)
        {
            return BindByName<string>(parameters);
        }

        /// <summary>
        /// Creates a new URI from the template and the collection of parameters.
        /// </summary>
        /// <param name="parameters">The parameter values.</param>
        /// <returns>A <see cref="Uri"/> object representing the expanded template.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="parameters"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">If a variable reference with a prefix modifier expands to a collection or dictionary.</exception>
        public Uri BindByName(IDictionary<string, object> parameters)
        {
            return BindByName<object>(parameters);
        }

        /// <summary>
        /// Creates a new URI from the template and the collection of parameters.
        /// </summary>
        /// <typeparam name="T">The type of parameter value provided in <paramref name="parameters"/>.</typeparam>
        /// <param name="parameters">The parameter values.</param>
        /// <returns>A <see cref="Uri"/> object representing the expanded template.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="parameters"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">If a variable reference with a prefix modifier expands to a collection or dictionary.</exception>
        public Uri BindByName<T>(IDictionary<string, T> parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");

            StringBuilder builder = new StringBuilder();
            foreach (UriTemplatePart part in _parts)
                part.Render(builder, parameters);

            return new Uri(builder.ToString(), UriKind.RelativeOrAbsolute);
        }

        /// <summary>
        /// Creates a new URI from the template and the collection of parameters. The URI formed
        /// by the expanded template is resolved against <paramref name="baseAddress"/> to produce
        /// an absolute URI.
        /// </summary>
        /// <param name="baseAddress">The base address of the URI.</param>
        /// <param name="parameters">The parameter values.</param>
        /// <returns>A <see cref="Uri"/> object representing the expanded template.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="baseAddress"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="parameters"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">If <paramref name="baseAddress"/> is not an absolute URI.</exception>
        public Uri BindByName(Uri baseAddress, IDictionary<string, string> parameters)
        {
            return BindByName<string>(baseAddress, parameters);
        }

        /// <summary>
        /// Creates a new URI from the template and the collection of parameters. The URI formed
        /// by the expanded template is resolved against <paramref name="baseAddress"/> to produce
        /// an absolute URI.
        /// </summary>
        /// <param name="baseAddress">The base address of the URI.</param>
        /// <param name="parameters">The parameter values.</param>
        /// <returns>A <see cref="Uri"/> object representing the expanded template.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="baseAddress"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="parameters"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">If <paramref name="baseAddress"/> is not an absolute URI.</exception>
        /// <exception cref="InvalidOperationException">If a variable reference with a prefix modifier expands to a collection or dictionary.</exception>
        public Uri BindByName(Uri baseAddress, IDictionary<string, object> parameters)
        {
            return BindByName<object>(baseAddress, parameters);
        }

        /// <summary>
        /// Creates a new URI from the template and the collection of parameters. The URI formed
        /// by the expanded template is resolved against <paramref name="baseAddress"/> to produce
        /// an absolute URI.
        /// </summary>
        /// <typeparam name="T">The type of parameter value provided in <paramref name="parameters"/>.</typeparam>
        /// <param name="baseAddress">The base address of the URI.</param>
        /// <param name="parameters">The parameter values.</param>
        /// <returns>A <see cref="Uri"/> object representing the expanded template.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="baseAddress"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="parameters"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">If <paramref name="baseAddress"/> is not an absolute URI.</exception>
        /// <exception cref="InvalidOperationException">If a variable reference with a prefix modifier expands to a collection or dictionary.</exception>
        public Uri BindByName<T>(Uri baseAddress, IDictionary<string, T> parameters)
        {
            if (baseAddress == null)
                throw new ArgumentNullException("baseAddress");
            if (parameters == null)
                throw new ArgumentNullException("parameters");
            if (!baseAddress.IsAbsoluteUri)
                throw new ArgumentException("baseAddress must be an absolute URI", "baseAddress");

            return new Uri(baseAddress, BindByName(parameters));
        }

        /// <summary>
        /// Attempts to match a <see cref="Uri"/> to a <see cref="UriTemplate"/>.
        /// </summary>
        /// <remarks>
        /// For detailed information about the behavior of this method, see the remarks in the
        /// documentation for the <see cref="O:Rackspace.Net.UriTemplate.Match"/> methods.
        /// </remarks>
        /// <param name="candidate">The <see cref="Uri"/> to match against the template.</param>
        /// <returns>A <see cref="UriTemplateMatch"/> object containing the results of the match operation, or <see langword="null"/> if the match failed.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="candidate"/> is <see langword="null"/>.
        /// </exception>
        /// <conceptualLink target="7e56a038-ad98-4922-9342-7f68a5b89283"/>
        public UriTemplateMatch Match(Uri candidate)
        {
            return Match(candidate, new string[0], new string[0], new string[0]);
        }

        /// <summary>
        /// Attempts to match a <see cref="Uri"/> to a <see cref="UriTemplate"/>.
        /// </summary>
        /// <remarks>
        /// For detailed information about the behavior of this method, see the remarks in the
        /// documentation for the <see cref="O:Rackspace.Net.UriTemplate.Match"/> methods.
        /// </remarks>
        /// <param name="candidate">The <see cref="Uri"/> to match against the template.</param>
        /// <param name="requiredVariables">A collection of variables which must be provided during the expansion process for the resulting URI to be valid.</param>
        /// <returns>A <see cref="UriTemplateMatch"/> object containing the results of the match operation, or <see langword="null"/> if the match failed. The default value is an empty collection.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="candidate"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="requiredVariables"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="requiredVariables"/> contains a <see langword="null"/> or empty value.
        /// </exception>
        /// <conceptualLink target="7e56a038-ad98-4922-9342-7f68a5b89283"/>
        public UriTemplateMatch Match(Uri candidate, ICollection<string> requiredVariables)
        {
            return Match(candidate, requiredVariables, new string[0], new string[0]);
        }

        /// <summary>
        /// Attempts to match a <see cref="Uri"/> to a <see cref="UriTemplate"/>.
        /// </summary>
        /// <remarks>
        /// For detailed information about the behavior of this method, see the remarks in the
        /// documentation for the <see cref="O:Rackspace.Net.UriTemplate.Match"/> methods.
        /// </remarks>
        /// <param name="candidate">The <see cref="Uri"/> to match against the template.</param>
        /// <param name="arrayVariables">A collection of variables to treat as associative arrays when matching a candidate URI to the template. Associative arrays are returned as instances of <see cref="IList{T}"/> whose values are of type <see cref="String"/>. The default value is an empty collection.</param>
        /// <param name="mapVariables">A collection of variables to treat as associative maps when matching a candidate URI to the template. Associative maps are returned as instances of <see cref="IDictionary{TKey, TValue}"/> whose keys and values are of type <see cref="String"/>. The default value is an empty collection.</param>
        /// <returns>A <see cref="UriTemplateMatch"/> object containing the results of the match operation, or <see langword="null"/> if the match failed.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="candidate"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="arrayVariables"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="mapVariables"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="arrayVariables"/> contains a <see langword="null"/> or empty value.
        /// <para>-or-</para>
        /// <para>If <paramref name="mapVariables"/> contains a <see langword="null"/> or empty value.</para>
        /// </exception>
        /// <conceptualLink target="7e56a038-ad98-4922-9342-7f68a5b89283"/>
        public UriTemplateMatch Match(Uri candidate, ICollection<string> arrayVariables, ICollection<string> mapVariables)
        {
            return Match(candidate, new string[0], arrayVariables, mapVariables);
        }

        /// <overloads>
        /// <summary>
        /// Attempts to match a <see cref="Uri"/> to a <see cref="UriTemplate"/>. A successful
        /// match operation results in an assignment of values to variables in the URI Template
        /// which is capable of producing a <c>candidate</c> URI through the
        /// <see cref="O:Rackspace.Net.UriTemplate.BindByName"/> operation.
        /// </summary>
        /// <remarks>
        /// There are several limitations in the current implementation of this operation.
        ///
        /// <list type="bullet">
        ///   <item>
        ///     If more than one assignment of values to variables exists which is capable of
        ///     producing the <c>candidate</c> URI through the <see cref="O:Rackspace.Net.UriTemplate.BindByName"/>
        ///     operation, it is unspecified which assignment of values is chosen.
        ///   </item>
        ///   <item>
        ///     Simple string values will always be returned as a <see cref="String"/>.
        ///     Associative array values will always be returned as an <see cref="IList{T}"/> whose
        ///     values are of type <see cref="String"/>. Associative map values will always be
        ///     returned as an <see cref="IDictionary{TKey, TValue}"/> whose keys and values are
        ///     of type <see cref="String"/>. No other deserialization or coercion of values is
        ///     performed by this library.
        ///   </item>
        /// </list>
        ///
        /// <para>
        /// The matching algorithm prefers to use simple string values for all variables not
        /// explicitly listed in <c>arrayVariables</c> and/or <c>mapVariables</c>. Any variable
        /// listed in either of these parameters will not be considered for matching as a simple
        /// string. If no assignment of values to variables is possible using this choice, one or
        /// more variables may be treated as associative arrays and/or maps in order to produce a
        /// successful assignment. The exception to this rule is compound template variables
        /// (which use the explode modifier); these variables prefer to match as arrays instead
        /// of simple strings, even if the result produces an array containing exactly one
        /// string. If a variable appears in both <c>arrayVariables</c> and <c>mapVariables</c>
        /// and the result successfully matches using both options, it is unspecified which one
        /// will be returned.
        /// </para>
        /// </remarks>
        /// <conceptualLink target="7e56a038-ad98-4922-9342-7f68a5b89283"/>
        /// </overloads>
        /// <summary>
        /// Attempts to match a <see cref="Uri"/> to a <see cref="UriTemplate"/>.
        /// </summary>
        /// <remarks>
        /// For detailed information about the behavior of this method, see the remarks in the
        /// documentation for the <see cref="O:Rackspace.Net.UriTemplate.Match"/> methods.
        /// </remarks>
        /// <param name="candidate">The <see cref="Uri"/> to match against the template.</param>
        /// <param name="requiredVariables">A collection of variables which must be provided during the expansion process for the resulting URI to be valid. The default value is an empty collection.</param>
        /// <param name="arrayVariables">A collection of variables to treat as associative arrays when matching a candidate URI to the template. Associative arrays are returned as instances of <see cref="IList{T}"/> whose values are of type <see cref="String"/>. The default value is an empty collection.</param>
        /// <param name="mapVariables">A collection of variables to treat as associative maps when matching a candidate URI to the template. Associative maps are returned as instances of <see cref="IDictionary{TKey, TValue}"/> whose keys and values are of type <see cref="String"/>. The default value is an empty collection.</param>
        /// <returns>A <see cref="UriTemplateMatch"/> object containing the results of the match operation, or <see langword="null"/> if the match failed.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="candidate"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="requiredVariables"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="arrayVariables"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="mapVariables"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="requiredVariables"/> contains a <see langword="null"/> or empty value.
        /// <para>-or-</para>
        /// <para>If <paramref name="arrayVariables"/> contains a <see langword="null"/> or empty value.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="mapVariables"/> contains a <see langword="null"/> or empty value.</para>
        /// </exception>
        /// <conceptualLink target="7e56a038-ad98-4922-9342-7f68a5b89283"/>
        public UriTemplateMatch Match(Uri candidate, ICollection<string> requiredVariables, ICollection<string> arrayVariables, ICollection<string> mapVariables)
        {
            if (candidate == null)
                throw new ArgumentNullException("candidate");
            if (requiredVariables == null)
                throw new ArgumentNullException("requiredVariables");
            if (arrayVariables == null)
                throw new ArgumentNullException("arrayVariables");
            if (mapVariables == null)
                throw new ArgumentNullException("mapVariables");

            StringBuilder pattern = new StringBuilder();
            pattern.Append('^');
            for (int i = 0; i < _parts.Length; i++)
            {
                string group = "part" + i;
                _parts[i].BuildPattern(pattern, group, requiredVariables, arrayVariables, mapVariables);
            }

            pattern.Append('$');

            Regex expression = new Regex(pattern.ToString());
            Match match = expression.Match(candidate.ToString());
            if (match == null || !match.Success)
                return null;

            List<KeyValuePair<VariableReference, object>> bindings = new List<KeyValuePair<VariableReference, object>>();
            for (int i = 0; i < _parts.Length; i++)
            {
                Group group = match.Groups["part" + i];
                if (!group.Success)
                    return null;

                KeyValuePair<VariableReference, object>[] binding = _parts[i].Match(group.Value, requiredVariables, arrayVariables, mapVariables);
                if (binding == null)
                    return null;

                bindings.AddRange(binding);
            }

            return new UriTemplateMatch(this, bindings);
        }

        /// <summary>
        /// Attempts to match a <see cref="Uri"/> to a <see cref="UriTemplate"/>.
        /// </summary>
        /// <param name="baseAddress">The base address.</param>
        /// <param name="candidate">The <see cref="Uri"/> to match against the template.</param>
        /// <returns>A <see cref="UriTemplateMatch"/> object containing the results of the match operation, or <see langword="null"/> if the match failed.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="baseAddress"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="candidate"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="baseAddress"/> is a relative URI.
        /// <para>-or-</para>
        /// <para>If <paramref name="candidate"/> is a relative URI.</para>
        /// </exception>
        public UriTemplateMatch Match(Uri baseAddress, Uri candidate)
        {
            if (baseAddress == null)
                throw new ArgumentNullException("baseAddress");
            if (candidate == null)
                throw new ArgumentNullException("candidate");

            Uri relative = baseAddress.MakeRelativeUri(candidate);
            return Match(relative);
        }

        /// <summary>
        /// Returns a string representation of the URI template, in the format described in RFC 6570.
        /// </summary>
        /// <inheritdoc/>
        public override string ToString()
        {
            return _template;
        }
    }
}
