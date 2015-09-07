// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace TunnelVisionLabs.Net
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Represents a literal in a URI Template.
    /// </summary>
    /// <seealso href="http://tools.ietf.org/html/rfc6570#section-2.1">Literals (RFC 6570 URI Template)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    internal sealed class UriTemplatePartLiteral : UriTemplatePart
    {
        /// <summary>
        /// This anchored regular expression matches a string that conforms to a sequence of <c>literals</c>.
        /// </summary>
        private static readonly Regex _literalSyntax = new Regex(@"^(?:[\x21\x23\x24\x26\x28-\x3B\x3D\x3F-\x5B\x5D\x5F\x61-\x7A\x7E\xA0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF\uE000-\uF8FF]|%[a-fA-F0-9]{2})*$", InternalRegexOptions.Default);

        /// <summary>
        /// This singleton is an empty array which is returned by <see cref="Match"/>.
        /// </summary>
        private static readonly KeyValuePair<VariableReference, object>[] EmptyMatches = new KeyValuePair<VariableReference, object>[0];

        /// <summary>
        /// This is the backing field for the <see cref="Text"/> property.
        /// </summary>
        private readonly string _text;

        /// <summary>
        /// Initializes a new instance of the <see cref="UriTemplatePartLiteral"/> class
        /// with the specified literal text.
        /// </summary>
        /// <param name="text">The literal text.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="text"/> is <see langword="null"/>.</exception>
        /// <exception cref="FormatException">If <paramref name="text"/> is not a sequence of <c>literals</c>, as defined by RFC 6570.</exception>
        public UriTemplatePartLiteral(string text)
        {
            if (text == null)
                throw new ArgumentNullException("text");
            if (!_literalSyntax.IsMatch(text))
                throw new FormatException(string.Format("Invalid URI template literal: {0}", text));

            _text = text;
        }

        /// <summary>
        /// Gets the raw text of this literal.
        /// </summary>
        /// <value>The raw text of this literal.</value>
        public string Text
        {
            get
            {
                return _text;
            }
        }

        /// <inheritdoc/>
        /// <value>This method always returns <see cref="UriTemplatePartType.Literal"/>.</value>
        public override UriTemplatePartType Type
        {
            get
            {
                return UriTemplatePartType.Literal;
            }
        }

        /// <inheritdoc/>
        /// <remarks>
        /// <para>This part is rendered by simply appending <see cref="Text"/> to <paramref name="builder"/>.</para>
        /// </remarks>
        public override void Render<T>(StringBuilder builder, IDictionary<string, T> parameters)
        {
            builder.Append(_text);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return _text;
        }

        /// <inheritdoc/>
        protected override void BuildPatternBody(StringBuilder pattern, ICollection<string> requiredVariables, ICollection<string> arrayVariables, ICollection<string> mapVariables)
        {
            pattern.Append(Regex.Escape(_text));
        }

        /// <inheritdoc/>
        /// <remarks>
        /// <para>Since a literal URI part cannot have variable references, this method simply returns an empty
        /// array.</para>
        /// </remarks>
        protected internal override KeyValuePair<VariableReference, object>[] Match(string text, ICollection<string> requiredVariables, ICollection<string> arrayVariables, ICollection<string> mapVariables)
        {
            return EmptyMatches;
        }
    }
}
