// Copyright (c) Rackspace, US Inc. All Rights Reserved. Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Rackspace.Net
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using DictionaryEntry = System.Collections.DictionaryEntry;
    using IDictionary = System.Collections.IDictionary;
    using IEnumerable = System.Collections.IEnumerable;

    /// <summary>
    /// Represents a URI Template expression of the form <c>{?x,y}</c> or <c>{&amp;x,y}</c>.
    /// </summary>
    internal sealed class UriTemplatePartQueryExpansion : UriTemplatePartExpansion
    {
        private readonly bool _continuation;

        /// <summary>
        /// Initializes a new instance of the <see cref="UriTemplatePartQueryExpansion"/> class
        /// with the specified values.
        /// </summary>
        /// <param name="variables">The variable references within this expression.</param>
        /// <param name="continuation"><see langword="true"/> if this is a query continuation expression; otherwise, <see langword="false"/>.</param>
        public UriTemplatePartQueryExpansion(IEnumerable<VariableReference> variables, bool continuation)
            : base(variables)
        {
            _continuation = continuation;
        }

        /// <inheritdoc/>
        /// <value>
        /// <see cref="UriTemplatePartType.Query"/> for templates of the form <c>{?x,y}</c>.
        /// <para>-or-</para>
        /// <para><see cref="UriTemplatePartType.QueryContinuation"/> for templates of the form <c>{&amp;x,y}</c>.</para>
        /// </value>
        public override UriTemplatePartType Type
        {
            get
            {
                return _continuation ? UriTemplatePartType.QueryContinuation : UriTemplatePartType.Query;
            }
        }

        protected override void RenderElement(StringBuilder builder, VariableReference variable, object variableValue, bool first)
        {
            RenderElement(builder, variable, variableValue, first, true);
        }

        protected override void RenderEnumerable(StringBuilder builder, VariableReference variable, IEnumerable variableValue, bool first)
        {
            bool firstElement = true;
            foreach (object value in variableValue)
            {
                if (value == null)
                    continue;

                RenderElement(builder, variable, value, first, firstElement);
                firstElement = false;
            }
        }

        protected override void RenderDictionary(StringBuilder builder, VariableReference variable, IDictionary variableValue, bool first)
        {
            bool firstElement = true;
            foreach (DictionaryEntry entry in variableValue)
            {
                if (variable.Composite)
                {
                    builder.Append(Type == UriTemplatePartType.Query && first && firstElement ? '?' : '&');
                    AppendText(builder, variable, entry.Key.ToString(), true);
                    builder.Append('=');
                    AppendText(builder, variable, entry.Value.ToString(), true);
                }
                else
                {
                    RenderElement(builder, variable, entry.Key, first, firstElement);
                    RenderElement(builder, variable, entry.Value, first, false);
                }

                firstElement = false;
            }
        }

        private void RenderElement(StringBuilder builder, VariableReference variable, object variableValue, bool firstVariable, bool firstElement)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");
            if (variableValue == null)
                throw new ArgumentNullException("variableValue");

            if (firstElement || variable.Composite)
                builder.Append(Type == UriTemplatePartType.Query && firstVariable && firstElement ? '?' : '&').Append(variable.Name).Append('=');
            else if (!firstElement)
                builder.Append(',');

            AppendText(builder, variable, variableValue.ToString(), true);
        }

        public override string ToString()
        {
            return string.Format("{{{0}{1}}}", Type == UriTemplatePartType.Query ? '?' : '&', string.Join(",", Variables.Select(i => i.Name).ToArray()));
        }
    }
}
