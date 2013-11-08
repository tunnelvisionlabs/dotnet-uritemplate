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

        private static readonly Regex ExpressionExpression =
            new Regex(@"{(?<Operator>" + OperatorPattern + @")?(?<VariableList>" + VariableListPattern + @")}", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>
        /// This is the backing field for the <see cref="Template"/> property.
        /// </summary>
        public readonly string _template;

        private readonly UriTemplatePart[] _parts;

        /// <summary>
        /// Initializes a new instance of the <see cref="UriTemplate"/> class
        /// using the specified template.
        /// </summary>
        /// <exception cref="ArgumentNullException">If <paramref name="template"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="template"/> is empty.</exception>
        /// <exception cref="FormatException">If <paramref name="template"/> is not a valid <c>URI-Template</c> according to RFC 6570.</exception>
        public UriTemplate(string template)
        {
            if (template == null)
                throw new ArgumentNullException("template");
            if (string.IsNullOrEmpty(template))
                throw new ArgumentException("template cannot be empty");

            _template = template;
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
                            throw new NotSupportedException(string.Format("Unrecognized operator: {0}", op.Value));
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
                        part = new UriTemplatePartSimpleExpansion(variables);
                        break;

                    case UriTemplatePartType.ReservedStringExpansion:
                        part = new UriTemplatePartReservedExpansion(variables);
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
                        throw new InvalidOperationException();
                }

                parts.Add(part);
                previousEnd = match.Index + match.Length;
            }

            if (previousEnd < template.Length)
                parts.Add(new UriTemplatePartLiteral(template.Substring(previousEnd)));

            _parts = parts.ToArray();
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

        public Uri BindByName(IDictionary<string, object> parameters)
        {
            StringBuilder builder = new StringBuilder();
            foreach (UriTemplatePart part in _parts)
                part.Render(builder, parameters);

            return new Uri(builder.ToString(), UriKind.RelativeOrAbsolute);
        }

        public Uri BindByName(Uri baseAddress, IDictionary<string, object> parameters)
        {
            return new Uri(baseAddress, BindByName(parameters));
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
