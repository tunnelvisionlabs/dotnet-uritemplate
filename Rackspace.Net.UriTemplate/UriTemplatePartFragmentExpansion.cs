// Copyright (c) Rackspace, US Inc. All Rights Reserved. Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Rackspace.Net
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
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

        /// <inheritdoc/>
        protected override void BuildPatternBodyImpl(StringBuilder pattern, ICollection<string> arrayVariables, ICollection<string> mapVariables)
        {
            if (pattern == null)
                throw new ArgumentNullException("pattern");
            if (arrayVariables == null)
                throw new ArgumentNullException("arrayVariables");
            if (mapVariables == null)
                throw new ArgumentNullException("mapVariables");

            List<string> variablePatterns = new List<string>();
            foreach (var variable in Variables)
            {
                bool allowReservedSet = true;
                variablePatterns.Add(BuildVariablePattern(variable, allowReservedSet, null, arrayVariables, mapVariables));
            }

            pattern.Append("(?:#");
            AppendOneOrMoreToEnd(pattern, variablePatterns, 0);
            pattern.Append(")?");
        }

        private static string BuildVariablePattern(VariableReference variable, bool allowReservedSet, string groupName, ICollection<string> arrayVariables, ICollection<string> mapVariables)
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

        private static void AppendOneOrMoreToEnd(StringBuilder pattern, List<string> patterns, int startIndex)
        {
            if (startIndex >= patterns.Count)
                throw new ArgumentException();

            pattern.Append("(?:");

            if (startIndex < patterns.Count - 1)
            {
                // include the first item and at least one more from there to the end
                pattern.Append(patterns[startIndex]).Append(",");
                AppendOneOrMoreToEnd(pattern, patterns, startIndex + 1);
                pattern.Append("|");
            }

            // include the first item alone
            pattern.Append(patterns[startIndex]);

            if (startIndex < patterns.Count - 1)
            {
                // don't include the first item, but do include one or more to the end
                pattern.Append("|");
                AppendOneOrMoreToEnd(pattern, patterns, startIndex + 1);
            }

            pattern.Append(")");
        }

        protected internal override KeyValuePair<VariableReference, object>[] Match(string text, ICollection<string> arrayVariables, ICollection<string> mapVariables)
        {
            List<string> variablePatterns = new List<string>();
            for (int i = 0; i < Variables.Count; i++)
            {
                bool allowReservedSet = true;
                variablePatterns.Add(BuildVariablePattern(Variables[i], allowReservedSet, "var" + i, arrayVariables, mapVariables));
            }

            StringBuilder matchPattern = new StringBuilder();
            matchPattern.Append("^#");
            AppendOneOrMoreToEnd(matchPattern, variablePatterns, 0);
            matchPattern.Append("$");

            Regex matchExpression = new Regex(matchPattern.ToString());
            Match match = matchExpression.Match(text);

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
                    Debug.Assert(considerMap);
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
                    Debug.Assert(considerArray);
                    List<string> list = new List<string>(group.Captures.Cast<Capture>().Select(capture => DecodeCharacters(capture.Value)));
                    results.Add(new KeyValuePair<VariableReference, object>(Variables[i], list));
                    continue;
                }

                Debug.Assert(considerString);
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
            return string.Format("{{#{0}}}", string.Join(",", Variables.Select(i => i.Name).ToArray()));
        }
    }
}
