// Copyright (c) Rackspace, US Inc. All Rights Reserved. Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Rackspace.Net
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using DictionaryEntry = System.Collections.DictionaryEntry;
    using IDictionary = System.Collections.IDictionary;
    using IEnumerable = System.Collections.IEnumerable;

    /// <summary>
    /// Represents a URI Template expression of the form <c>{.x,y}</c>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    internal sealed class UriTemplatePartLabelExpansion : UriTemplatePartExpansion
    {
        public UriTemplatePartLabelExpansion(IEnumerable<VariableReference> variables)
            : base(variables)
        {
        }

        /// <inheritdoc/>
        /// <value>This method always returns <see cref="UriTemplatePartType.LabelExpansion"/>.</value>
        public override UriTemplatePartType Type
        {
            get
            {
                return UriTemplatePartType.LabelExpansion;
            }
        }

        protected override void BuildPatternBodyImpl(StringBuilder pattern, ICollection<string> listVariables, ICollection<string> mapVariables)
        {
            for (int i = 0; i < Variables.Count; i++)
            {
                if (Variables[i].Prefix != null)
                    throw new NotImplementedException("Matching prefix variables is not yet supported");
                if (listVariables.Contains(Variables[i].Name))
                    throw new NotImplementedException("Matching list variables is not yet supported");
                if (mapVariables.Contains(Variables[i].Name))
                    throw new NotImplementedException("Matching associative map variables is not yet supported");

                pattern.Append("(?:");
                pattern.Append(Regex.Escape("."));
                pattern.Append(UnreservedCharacterPattern).Append('*');
            }

            for (int i = 0; i < Variables.Count; i++)
            {
                pattern.Append(")?");
            }
        }

        protected internal override KeyValuePair<VariableReference, object>[] Match(string text, ICollection<string> listVariables, ICollection<string> mapVariables)
        {
            if (string.IsNullOrEmpty(text))
                return new KeyValuePair<VariableReference, object>[0];

            if (Variables.Count > 1)
                throw new NotSupportedException("Matching more than one label variable is not supported");

            if (text[0] != '.')
                throw new FormatException("The specified text is not a valid label expansion");

            text = text.Substring(1);

            for (int i = 0; i < Variables.Count; i++)
            {
                if (Variables[i].Prefix != null)
                    throw new NotImplementedException("Matching prefix variables is not yet supported");
                if (listVariables.Contains(Variables[i].Name))
                    throw new NotImplementedException("Matching list variables is not yet supported");
                if (mapVariables.Contains(Variables[i].Name))
                    throw new NotImplementedException("Matching associative map variables is not yet supported");
            }

            List<KeyValuePair<VariableReference, object>> bindings = new List<KeyValuePair<VariableReference, object>>();
            bindings.Add(new KeyValuePair<VariableReference, object>(Variables[0], DecodeCharacters(text)));
            return bindings.ToArray();
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
                    builder.Append('.');
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
                builder.Append('.');
            else if (!firstElement)
                builder.Append(',');

            AppendText(builder, variable, variableValue.ToString(), true);
        }

        public override string ToString()
        {
            return string.Format("{{.{0}}}", string.Join(",", Variables.Select(i => i.Name).ToArray()));
        }
    }
}
