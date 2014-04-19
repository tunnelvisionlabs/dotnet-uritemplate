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
    /// Represents a URI Template expression of the form <c>{x,y}</c> or <c>{+x,y}</c>.
    /// </summary>
    internal sealed class UriTemplatePartSimpleExpansion : UriTemplatePartExpansion
    {
        private readonly bool _escapeReserved;

        public UriTemplatePartSimpleExpansion(IEnumerable<VariableReference> variables, bool escapeReserved)
            : base(variables)
        {
            _escapeReserved = escapeReserved;
        }

        public override UriTemplatePartType Type
        {
            get
            {
                return _escapeReserved ? UriTemplatePartType.SimpleStringExpansion : UriTemplatePartType.ReservedStringExpansion;
            }
        }

        protected override void RenderElement(StringBuilder builder, VariableReference variable, object variableValue, bool first)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");
            if (variableValue == null)
                throw new ArgumentNullException("variableValue");

            if (!first)
                builder.Append(',');

            AppendText(builder, variable, variableValue.ToString(), _escapeReserved);
        }

        protected override void RenderEnumerable(StringBuilder builder, VariableReference variable, IEnumerable variableValue, bool first)
        {
            foreach (object value in variableValue)
            {
                if (value == null)
                    continue;

                RenderElement(builder, variable, value, first);
                first = false;
            }
        }

        protected override void RenderDictionary(StringBuilder builder, VariableReference variable, IDictionary variableValue, bool first)
        {
            foreach (DictionaryEntry entry in variableValue)
            {
                if (variable.Composite)
                {
                    if (!first)
                        builder.Append(',');

                    AppendText(builder, variable, entry.Key.ToString(), _escapeReserved);
                    builder.Append('=');
                    AppendText(builder, variable, entry.Value.ToString(), _escapeReserved);
                }
                else
                {
                    RenderElement(builder, variable, entry.Key, first);
                    RenderElement(builder, variable, entry.Value, false);
                }

                first = false;
            }
        }

        public override string ToString()
        {
            return string.Format("{{{0}{1}}}", Type == UriTemplatePartType.SimpleStringExpansion ? string.Empty : "+", string.Join(",", Variables.Select(i => i.Name).ToArray()));
        }
    }
}
