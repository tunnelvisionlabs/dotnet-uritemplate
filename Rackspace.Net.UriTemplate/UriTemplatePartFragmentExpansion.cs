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
    /// Represents a URI Template expression of the form <c>{#x,y}</c>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    internal sealed class UriTemplatePartFragmentExpansion : UriTemplatePartExpansion
    {
        public UriTemplatePartFragmentExpansion(IEnumerable<VariableReference> variables)
            : base(variables)
        {
        }

        /// <inheritdoc/>
        /// <value>This method always returns <see cref="UriTemplatePartType.FragmentExpansion"/>.</value>
        public override UriTemplatePartType Type
        {
            get
            {
                return UriTemplatePartType.FragmentExpansion;
            }
        }

        protected override void BuildPatternBodyImpl(StringBuilder pattern, ICollection<string> listVariables, ICollection<string> mapVariables)
        {
            pattern.Append("(?:");
            pattern.Append(Regex.Escape("#"));
            pattern.Append("(?:");
            pattern.Append(UnreservedCharacterPattern);
            pattern.Append('|');
            pattern.Append(ReservedCharacterPattern);
            pattern.Append(")*");
            pattern.Append(")?");
        }

        protected internal override KeyValuePair<VariableReference, object>[] Match(string text, ICollection<string> listVariables, ICollection<string> mapVariables)
        {
            if (string.IsNullOrEmpty(text))
                return new KeyValuePair<VariableReference, object>[0];

            if (Variables.Count > 1)
                throw new NotSupportedException("Matching more than one fragment variable is not supported");

            if (text[0] != '#')
                throw new FormatException("The specified text is not a valid fragment expansion");

            text = text.Substring(1);

            List<KeyValuePair<VariableReference, object>> bindings = new List<KeyValuePair<VariableReference, object>>();

            VariableReference variable = Variables[0];
            if (listVariables.Contains(variable.Name))
            {
                string[] values = text.Split(',');
                bindings.Add(new KeyValuePair<VariableReference, object>(variable, values.ConvertAll(DecodeCharacters)));
            }
            else if (mapVariables.Contains(variable.Name))
            {
                if (variable.Composite)
                {
                    Regex expression = new Regex(@"^(?<Key>.*?)=(?<Value>.*?)(?:,(?<Key>.*?)=(?<Value>.*?))*$");
                    Match match = expression.Match(text);
                    if (!match.Success)
                        throw new FormatException();

                    Dictionary<string, string> map = new Dictionary<string, string>();
                    Group keys = match.Groups["Key"];
                    Group values = match.Groups["Value"];
                    for (int i = 0; i < keys.Captures.Count; i++)
                        map.Add(DecodeCharacters(keys.Captures[i].Value), DecodeCharacters(values.Captures[i].Value));

                    bindings.Add(new KeyValuePair<VariableReference, object>(variable, map));
                }
                else
                {
                    string[] values = text.Split(',');
                    if ((values.Length % 2) != 0)
                        throw new FormatException();

                    Dictionary<string, string> map = new Dictionary<string, string>();
                    for (int i = 0; i < values.Length; i += 2)
                        map.Add(DecodeCharacters(values[i]), DecodeCharacters(values[i + 1]));

                    bindings.Add(new KeyValuePair<VariableReference, object>(variable, map));
                }
            }
            else
            {
                bindings.Add(new KeyValuePair<VariableReference, object>(variable, DecodeCharacters(text)));
            }

            if (variable.Prefix != null)
            {
                throw new NotImplementedException("Matching prefix variables is not yet supported");
            }

            return bindings.ToArray();
        }

        protected override void RenderElement(StringBuilder builder, VariableReference variable, object variableValue, bool first)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");
            if (variableValue == null)
                throw new ArgumentNullException("variableValue");

            if (first)
                builder.Append('#');
            if (!first)
                builder.Append(',');

            AppendText(builder, variable, variableValue.ToString(), false);
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

            if (first)
                builder.Append('#');
        }

        protected override void RenderDictionary(StringBuilder builder, VariableReference variable, IDictionary variableValue, bool first)
        {
            foreach (DictionaryEntry entry in variableValue)
            {
                if (variable.Composite)
                {
                    if (first)
                        builder.Append('#');
                    else
                        builder.Append(',');

                    AppendText(builder, variable, entry.Key.ToString(), false);
                    builder.Append('=');
                    AppendText(builder, variable, entry.Value.ToString(), false);
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
            return string.Format("{{#{0}}}", string.Join(",", Variables.Select(i => i.Name).ToArray()));
        }
    }
}
