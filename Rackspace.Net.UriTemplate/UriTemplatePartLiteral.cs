namespace Rackspace.Net
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;

    internal sealed class UriTemplatePartLiteral : UriTemplatePart
    {
#if !PORTABLE
        private const RegexOptions DefaultRegexOptions = RegexOptions.Compiled | RegexOptions.CultureInvariant;
#else
        private const RegexOptions DefaultRegexOptions = RegexOptions.CultureInvariant;
#endif

        private static readonly Regex _literalSyntax = new Regex(@"^(?:[\x21\x23\x24\x26\x28-\x3B\x3D\x3F-\x5B\x5D\x5F\x61-\x7A\x7E\xA0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF\uE000-\uF8FF]|%[a-fA-F0-9]{2})*$", DefaultRegexOptions);

        private readonly string _text;

        public UriTemplatePartLiteral(string text)
        {
            if (text == null)
                throw new ArgumentNullException("text");
            if (!_literalSyntax.IsMatch(text))
                throw new FormatException(string.Format("Invalid URI template literal: {0}", text));

            _text = text;
        }

        public override UriTemplatePartType Type
        {
            get
            {
                return UriTemplatePartType.Literal;
            }
        }

        public override void Render(StringBuilder builder, IDictionary<string, object> parameters)
        {
            builder.Append(_text);
        }

        public override string ToString()
        {
            return _text;
        }
    }
}
