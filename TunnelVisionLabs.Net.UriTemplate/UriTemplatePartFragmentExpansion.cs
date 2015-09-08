// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace TunnelVisionLabs.Net
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using System.Text.RegularExpressions;
    using BitArray = System.Collections.BitArray;
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

        /// <inheritdoc/>
        protected override void BuildPatternBodyImpl(StringBuilder pattern, ICollection<string> requiredVariables, ICollection<string> arrayVariables, ICollection<string> mapVariables)
        {
            if (pattern == null)
                throw new ArgumentNullException("pattern");
            if (arrayVariables == null)
                throw new ArgumentNullException("arrayVariables");
            if (mapVariables == null)
                throw new ArgumentNullException("mapVariables");

            BitArray requiredPatterns = new BitArray(Variables.Count);
            List<string> variablePatterns = new List<string>();
            for (int i = 0; i < Variables.Count; i++)
            {
                VariableReference variable = Variables[i];
                if (requiredVariables.Contains(variable.Name))
                    requiredPatterns.Set(i, true);

                bool allowReservedSet = true;
                variablePatterns.Add(BuildVariablePattern(variable, allowReservedSet, null, requiredVariables, arrayVariables, mapVariables));
            }

            pattern.Append("(?:#");
            AppendOneOrMoreToEnd(pattern, requiredPatterns, variablePatterns, 0);
            pattern.Append(")?");
        }

        private static string BuildVariablePattern(VariableReference variable, bool allowReservedSet, string groupName, ICollection<string> requiredVariables, ICollection<string> arrayVariables, ICollection<string> mapVariables)
        {
            string characterPattern;
            if (allowReservedSet)
                characterPattern = "(?:" + UnreservedCharacterPattern + "|" + ReservedCharacterPattern + ")";
            else
                characterPattern = "(?:" + UnreservedCharacterPattern + ")";

            string valueStartPattern;
            if (!string.IsNullOrEmpty(groupName))
                valueStartPattern = "(?<" + groupName + ">";
            else
                valueStartPattern = "(?:";

            string valueEndPattern = ")";

            string keyStartPattern;
            if (!string.IsNullOrEmpty(groupName))
                keyStartPattern = "(?<" + groupName + "key>";
            else
                keyStartPattern = "(?:";

            string keyEndPattern = ")";

            string mapValueStartPattern;
            if (!string.IsNullOrEmpty(groupName))
                mapValueStartPattern = "(?<" + groupName + "value>";
            else
                mapValueStartPattern = "(?:";

            string mapValueEndPattern = ")";

            string countPattern;
            if (allowReservedSet)
                countPattern = "*?";
            else
                countPattern = "*";

            StringBuilder variablePattern = new StringBuilder();

            if (variable.Prefix != null)
            {
                // by this point we know to match the variable as a simple string
                variablePattern.Append(valueStartPattern);
                variablePattern.Append(characterPattern);
                variablePattern.Append("{0,").Append(variable.Prefix).Append("}");
                variablePattern.Append(valueEndPattern);
                return variablePattern.ToString();
            }

            bool treatAsArray = arrayVariables.Contains(variable.Name);
            bool treatAsMap = mapVariables.Contains(variable.Name);

            bool considerString = !variable.Composite && !treatAsArray && !treatAsMap;
            bool considerArray = treatAsArray || !treatAsMap;
            bool considerMap = treatAsMap || !treatAsArray;

            variablePattern.Append("(?:");

            if (considerString)
            {
                // could be a simple string
                variablePattern.Append(valueStartPattern);
                variablePattern.Append(characterPattern).Append(countPattern);
                variablePattern.Append(valueEndPattern);
            }

            if (considerArray)
            {
                if (considerString)
                    variablePattern.Append('|');

                // could be an associative array
                variablePattern.Append(valueStartPattern).Append(characterPattern).Append(countPattern).Append(valueEndPattern);
                variablePattern.Append("(?:,");
                variablePattern.Append(valueStartPattern).Append(characterPattern).Append(countPattern).Append(valueEndPattern);
                variablePattern.Append(")*?");
            }

            if (considerMap)
            {
                if (considerString || considerArray)
                    variablePattern.Append('|');

                // could be an associative map
                char separator = variable.Composite ? '=' : ',';
                variablePattern.Append(valueStartPattern);
                variablePattern.Append(keyStartPattern);
                variablePattern.Append(characterPattern).Append(countPattern);
                variablePattern.Append(keyEndPattern);
                variablePattern.Append(separator).Append(mapValueStartPattern).Append(characterPattern).Append(countPattern).Append(mapValueEndPattern);
                variablePattern.Append(valueEndPattern);
                variablePattern.Append("(?:,");
                variablePattern.Append(valueStartPattern);
                variablePattern.Append(keyStartPattern);
                variablePattern.Append(characterPattern).Append(countPattern);
                variablePattern.Append(keyEndPattern);
                variablePattern.Append(separator).Append(mapValueStartPattern).Append(characterPattern).Append(countPattern).Append(mapValueEndPattern);
                variablePattern.Append(valueEndPattern);
                variablePattern.Append(")*?");
            }

            variablePattern.Append(")");

            return variablePattern.ToString();
        }

        private static void AppendOneOrMoreToEnd(StringBuilder pattern, BitArray requiredPatterns, List<string> patterns, int startIndex)
        {
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException("startIndex cannot be negative", "startIndex");
            if (startIndex >= patterns.Count)
                throw new ArgumentException("startIndex cannot be greater than the number of patterns.", "startIndex");

            pattern.Append("(?:");

            if (requiredPatterns.Get(startIndex))
            {
                // include the required first item
                pattern.Append(patterns[startIndex]);

                if (startIndex < patterns.Count - 1)
                {
                    // optionally include at least one more from there to the end
                    pattern.Append("(?:,");
                    AppendOneOrMoreToEnd(pattern, requiredPatterns, patterns, startIndex + 1);
                    pattern.Append(")?");
                }
            }
            else
            {
                if (startIndex < patterns.Count - 1)
                {
                    // include the first item and at least one more from there to the end
                    pattern.Append(patterns[startIndex]).Append(",");
                    AppendOneOrMoreToEnd(pattern, requiredPatterns, patterns, startIndex + 1);
                    pattern.Append("|");
                }

                // include the first item alone
                pattern.Append(patterns[startIndex]);

                if (startIndex < patterns.Count - 1)
                {
                    // don't include the first item, but do include one or more to the end
                    pattern.Append("|");
                    AppendOneOrMoreToEnd(pattern, requiredPatterns, patterns, startIndex + 1);
                }
            }

            pattern.Append(")");
        }

        protected override KeyValuePair<VariableReference, object>[] MatchImpl(string text, ICollection<string> requiredVariables, ICollection<string> arrayVariables, ICollection<string> mapVariables)
        {
            BitArray requiredPatterns = new BitArray(Variables.Count);
            List<string> variablePatterns = new List<string>();
            for (int i = 0; i < Variables.Count; i++)
            {
                VariableReference variable = Variables[i];
                if (requiredVariables.Contains(variable.Name))
                    requiredPatterns.Set(i, true);

                bool allowReservedSet = true;
                variablePatterns.Add(BuildVariablePattern(Variables[i], allowReservedSet, "var" + i, requiredVariables, arrayVariables, mapVariables));
            }

            StringBuilder matchPattern = new StringBuilder();
            matchPattern.Append("^#");
            AppendOneOrMoreToEnd(matchPattern, requiredPatterns, variablePatterns, 0);
            matchPattern.Append("$");

            Match match = Regex.Match(text, matchPattern.ToString());

            List<KeyValuePair<VariableReference, object>> results = new List<KeyValuePair<VariableReference, object>>();
            for (int i = 0; i < Variables.Count; i++)
            {
                Group group = match.Groups["var" + i];
                if (!group.Success || group.Captures.Count == 0)
                    continue;

                if (Variables[i].Prefix != null)
                {
                    if (group.Success && group.Captures.Count == 1)
                    {
                        results.Add(new KeyValuePair<VariableReference, object>(Variables[i], DecodeCharacters(group.Captures[0].Value)));
                    }

                    continue;
                }

                bool treatAsArray = arrayVariables.Contains(Variables[i].Name);
                bool treatAsMap = mapVariables.Contains(Variables[i].Name);

                bool considerString = !Variables[i].Composite && !treatAsArray && !treatAsMap;
                bool considerArray = treatAsArray || !treatAsMap;
                bool considerMap = treatAsMap || !treatAsArray;

                // first check for a map
                Group mapKeys = match.Groups["var" + i + "key"];
                if (mapKeys.Success && mapKeys.Captures.Count > 0)
                {
                    Debug.Assert(considerMap, "considerMap");
                    Group mapValues = match.Groups["var" + i + "value"];
                    Dictionary<string, string> map = new Dictionary<string, string>();
                    for (int j = 0; j < mapKeys.Captures.Count; j++)
                        map.Add(DecodeCharacters(mapKeys.Captures[j].Value), DecodeCharacters(mapValues.Captures[j].Value));

                    results.Add(new KeyValuePair<VariableReference, object>(Variables[i], map));
                    continue;
                }

                // next try an array
                if (!considerString || group.Captures.Count > 1)
                {
                    Debug.Assert(considerArray, "considerArray");
                    List<string> list = new List<string>(group.Captures.Count);
                    foreach (Capture capture in group.Captures)
                        list.Add(DecodeCharacters(capture.Value));

                    results.Add(new KeyValuePair<VariableReference, object>(Variables[i], list));
                    continue;
                }

                Debug.Assert(considerString, "considerString");
                results.Add(new KeyValuePair<VariableReference, object>(Variables[i], DecodeCharacters(group.Captures[0].Value)));
            }

            return results.ToArray();
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
            List<string> names = new List<string>(Variables.Count);
            foreach (VariableReference variable in Variables)
                names.Add(variable.Name);

            return string.Format("{{#{0}}}", string.Join(",", names.ToArray()));
        }
    }
}
