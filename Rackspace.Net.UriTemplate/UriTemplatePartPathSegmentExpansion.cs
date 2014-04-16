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
    /// Represents a URI Template expression of the form <c>{/x,y}</c>.
    /// </summary>
    internal sealed class UriTemplatePartPathSegmentExpansion : UriTemplatePartExpansion
    {
        public UriTemplatePartPathSegmentExpansion(IEnumerable<VariableReference> variables)
            : base(variables)
        {
        }

        /// <inheritdoc/>
        /// <value>This method always returns <see cref="UriTemplatePartType.PathSegments"/>.</value>
        public override UriTemplatePartType Type
        {
            get
            {
                return UriTemplatePartType.PathSegments;
            }
        }

        protected override void BuildPatternBodyImpl(StringBuilder pattern, ICollection<string> listVariables, ICollection<string> mapVariables)
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
                pattern.Append(Regex.Escape("/"));
                pattern.Append(elementFormat);

                if (treatAsList || treatAsMap)
                {
                    pattern.Append("(?:");
                    pattern.Append(Regex.Escape(Variables[i].Composite ? "/" : ","));
                    pattern.Append(elementFormat);
                    pattern.Append(")*");
                }

                pattern.Append(")?");
            }
        }

        protected internal override KeyValuePair<VariableReference, object>[] Match(string text, ICollection<string> listVariables, ICollection<string> mapVariables)
        {
            if (string.IsNullOrEmpty(text))
                return new KeyValuePair<VariableReference, object>[0];

            if (text[0] != '/')
                throw new FormatException("The specified text is not a valid path segment expansion");

            text = text.Substring(1);

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

            List<KeyValuePair<VariableReference, object>> bindings = new List<KeyValuePair<VariableReference, object>>();
            string[] bound = text.Split('/');

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
                    if (bound.Length != 1)
                        throw new FormatException();

                    bound = bound[0].Split(',');
                    if ((bound.Length % 2) != 0)
                        throw new FormatException();

                    for (int i = 0; i < bound.Length; i += 2)
                    {
                        mapVariable.Add(DecodeCharacters(bound[i]), DecodeCharacters(bound[i + 1]));
                    }

                    bindings.Add(new KeyValuePair<VariableReference, object>(Variables[0], mapVariable));
                    return bindings.ToArray();
                }
            }

            if (listIndex >= 0)
            {
                if (Variables.Count != 1)
                    throw new NotImplementedException("Matching list variables with other variables is not yet supported.");

                if (Variables[listIndex].Composite)
                {
                    bindings.Add(new KeyValuePair<VariableReference, object>(Variables[0], bound.ConvertAll(DecodeCharacters)));
                }
                else
                {
                    if (bound.Length != 1)
                        throw new FormatException();

                    bound = bound[0].Split(',');
                    bindings.Add(new KeyValuePair<VariableReference, object>(Variables[0], bound.ConvertAll(DecodeCharacters)));
                }
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
            //List<KeyValuePair<VariableReference, object>> bindings = new List<KeyValuePair<VariableReference, object>>();
            //string[] bound = text.Split('/');
            //for (int i = 0; i < bound.Length; i++)
            //{
            //    bindings.Add(new KeyValuePair<VariableReference, object>(Variables[i], DecodeCharacters(bound[i])));
            //}

            //return bindings.ToArray();
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
                    builder.Append('/');
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
                builder.Append("/");
            else if (!firstElement)
                builder.Append(',');

            AppendText(builder, variable, variableValue.ToString(), true);
        }

        public override string ToString()
        {
            return string.Format("{{/{0}}}", string.Join(",", Variables.Select(i => i.Name).ToArray()));
        }
    }
}
