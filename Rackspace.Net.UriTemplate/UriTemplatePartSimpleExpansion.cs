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
    /// Represents a URI Template expression of the form <c>{x,y}</c> or <c>{+x,y}</c>.
    /// </summary>
    internal sealed class UriTemplatePartSimpleExpansion : UriTemplatePartExpansion
    {
        /// <summary>
        /// <see langword="true"/> to escape reserved characters during rendering; otherwise, <see langword="false"/>.
        /// </summary>
        private readonly bool _escapeReserved;

        public UriTemplatePartSimpleExpansion(IEnumerable<VariableReference> variables, bool escapeReserved)
            : base(variables)
        {
            _escapeReserved = escapeReserved;
        }

        /// <inheritdoc/>
        /// <value>
        /// <see cref="UriTemplatePartType.SimpleStringExpansion"/> for templates of the form <c>{x,y}</c>.
        /// <para>-or-</para>
        /// <para><see cref="UriTemplatePartType.ReservedStringExpansion"/> for templates of the form <c>{+x,y}</c>.</para>
        /// </value>
        public override UriTemplatePartType Type
        {
            get
            {
                return _escapeReserved ? UriTemplatePartType.SimpleStringExpansion : UriTemplatePartType.ReservedStringExpansion;
            }
        }

        /// <inheritdoc/>
        protected override void BuildPatternBodyImpl(StringBuilder pattern, ICollection<string> listVariables, ICollection<string> mapVariables)
        {
            if (Type == UriTemplatePartType.SimpleStringExpansion)
            {
                for (int i = 0; i < Variables.Count; i++)
                {
                    bool treatAsList = listVariables.Contains(Variables[i].Name);
                    bool treatAsMap = mapVariables.Contains(Variables[i].Name);

                    string countPattern = "*";
                    if (Variables[i].Prefix != null)
                        countPattern = string.Format("{{0,{0}}}", Variables[i].Prefix);

                    string elementFormat = UnreservedCharacterPattern + countPattern;
                    if (treatAsMap)
                        elementFormat = elementFormat + Regex.Escape(Variables[i].Composite ? "=" : ",") + elementFormat;

                    pattern.Append("(?:");
                    if (i > 0)
                        pattern.Append(Regex.Escape(","));

                    pattern.Append(elementFormat);

                    if (treatAsList || treatAsMap)
                    {
                        pattern.Append("(?:");
                        pattern.Append(Regex.Escape(","));
                        pattern.Append(elementFormat);
                        pattern.Append(")*");
                    }
                }

                for (int i = 0; i < Variables.Count; i++)
                {
                    pattern.Append(")?");
                }
            }
            else
            {
                for (int i = 0; i < Variables.Count; i++)
                {
                    if (listVariables.Contains(Variables[i].Name))
                        throw new NotImplementedException("Matching list variables is not yet supported");
                    if (mapVariables.Contains(Variables[i].Name))
                        throw new NotImplementedException("Matching associative map variables is not yet supported");

                    pattern.Append("(?:");
                    if (i > 0)
                        pattern.Append(Regex.Escape(","));

                    pattern.Append("(?:");
                    pattern.Append(UnreservedCharacterPattern);
                    pattern.Append('|');
                    pattern.Append(ReservedCharacterPattern);
                    pattern.Append(")*");
                }

                for (int i = 0; i < Variables.Count; i++)
                {
                    pattern.Append(")?");
                }
            }
        }

        protected internal override KeyValuePair<VariableReference, object>[] Match(string text, ICollection<string> listVariables, ICollection<string> mapVariables)
        {
            if (Type == UriTemplatePartType.SimpleStringExpansion)
            {
                int listIndex = -1;
                int mapIndex = -1;
                for (int i = 0; i < Variables.Count; i++)
                {
                    if (listVariables.Contains(Variables[i].Name))
                    {
                        if (listIndex >= 0)
                            throw new NotSupportedException("Matching multiple list variables in a single expansion is not supported.");

                        listIndex = i;
                    }
                    else if (mapVariables.Contains(Variables[i].Name))
                    {
                        if (mapIndex >= 0)
                            throw new NotSupportedException("Matching multiple map variables in a single expansion is not supported.");

                        mapIndex = i;
                    }
                }

                if (listIndex >= 0 && mapIndex >= 0)
                {
                    if (!Variables[mapIndex].Composite)
                        throw new NotSupportedException("Cannot match both a list and a map variable unless the map variable is composite");
                }

                List<KeyValuePair<VariableReference, object>> bindings = new List<KeyValuePair<VariableReference, object>>();
                string[] bound = text.Split(',');

                int mapStart = -1;
                int mapEnd = -1;
                Dictionary<string, string> mapVariable = mapIndex >= 0 ? new Dictionary<string, string>() : null;
                if (mapIndex >= 0)
                {
                    if (Variables[mapIndex].Composite)
                    {
                        for (int i = bound.FindIndex(x => x.IndexOf('=') >= 0); i >= 0 && i < bound.Length && bound[i].IndexOf('=') >= 0; i++)
                        {
                            if (mapVariable.Count == 0)
                                mapStart = i;

                            string[] keyValue = bound[i].Split('=');
                            if (keyValue.Length != 2)
                                throw new FormatException();

                            mapVariable.Add(DecodeCharacters(keyValue[0]), DecodeCharacters(keyValue[1]));
                        }

                        mapEnd = mapStart + mapVariable.Count;
                    }
                    else
                    {
                        if (Variables.Count != 1)
                            throw new NotImplementedException("Matching non-compound associative map variables with other variables is not yet supported.");
                        if ((bound.Length % 2) != 0)
                            throw new FormatException();

                        for (int i = 0; i < bound.Length; i += 2)
                        {
                            mapVariable.Add(DecodeCharacters(bound[i]), DecodeCharacters(bound[i + 1]));
                        }

                        mapStart = 0;
                        mapEnd = bound.Length;
                    }
                }

                if (listIndex >= 0)
                {
                    if (Variables.Count != 1)
                        throw new NotImplementedException("Matching list variables with other variables is not yet supported.");

                    bindings.Add(new KeyValuePair<VariableReference, object>(Variables[0], bound.ConvertAll(DecodeCharacters)));
                }
                else
                {
                    for (int i = 0; i < bound.Length; i++)
                    {
                        if (i == mapStart)
                        {
                            if (!mapVariables.Contains(Variables[i].Name))
                                throw new InvalidOperationException();

                            bindings.Add(new KeyValuePair<VariableReference, object>(Variables[i], mapVariable));
                            i = mapEnd - 1;
                            continue;
                        }

                        bindings.Add(new KeyValuePair<VariableReference, object>(Variables[i], DecodeCharacters(bound[i])));
                    }
                }

                return bindings.ToArray();
            }
            else
            {
                if (Variables.Count > 1)
                    throw new NotSupportedException("Matching more than one reserved variable is not supported");

                for (int i = 0; i < Variables.Count; i++)
                {
                    if (listVariables.Contains(Variables[i].Name))
                        throw new NotImplementedException("Matching list variables is not yet supported");
                    if (mapVariables.Contains(Variables[i].Name))
                        throw new NotImplementedException("Matching associative map variables is not yet supported");
                }

                List<KeyValuePair<VariableReference, object>> bindings = new List<KeyValuePair<VariableReference, object>>();
                string[] bound = text.Split(',');
                for (int i = 0; i < bound.Length; i++)
                {
                    string decodedValue = DecodeCharacters(bound[i]);
                    if (Variables[i].Prefix < decodedValue.Length)
                        throw new FormatException(string.Format("Variable '{0}' has a maximum length of {1}", Variables[i].Name, Variables[i].Prefix));

                    bindings.Add(new KeyValuePair<VariableReference, object>(Variables[i], decodedValue));
                }

                return bindings.ToArray();
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
