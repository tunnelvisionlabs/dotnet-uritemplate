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

        /// <inheritdoc/>
        protected override void BuildPatternBodyImpl(StringBuilder pattern, ICollection<string> listVariables, ICollection<string> mapVariables)
        {
            if (pattern == null)
                throw new ArgumentNullException("pattern");
            if (listVariables == null)
                throw new ArgumentNullException("listVariables");
            if (mapVariables == null)
                throw new ArgumentNullException("mapVariables");

            List<string> variablePatterns = new List<string>();
            foreach (var variable in Variables)
            {
                bool allowReservedSet = false;
                variablePatterns.Add(BuildVariablePattern(variable, allowReservedSet, null, listVariables, mapVariables));
            }

            pattern.Append("(?:").Append(Type == UriTemplatePartType.QueryContinuation ? "&" : Regex.Escape("?"));
            AppendOneOrMoreUnorderedToEnd(pattern, variablePatterns, 0);
            pattern.Append(")?");
        }

        private static string BuildVariablePattern(VariableReference variable, bool allowReservedSet, string groupName, ICollection<string> listVariables, ICollection<string> mapVariables)
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

            string nameStartPattern;
            if (!string.IsNullOrEmpty(groupName))
                nameStartPattern = "(?<" + groupName + "name>";
            else
                nameStartPattern = "(?:";

            string nameEndPattern = ")";

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
                variablePattern.Append("(?:");
                variablePattern.Append(nameStartPattern).Append(Regex.Escape(variable.Name)).Append(nameEndPattern).Append('=');
                variablePattern.Append(valueStartPattern);
                variablePattern.Append(characterPattern);
                variablePattern.Append("{0,").Append(variable.Prefix).Append("}");
                variablePattern.Append(valueEndPattern);
                variablePattern.Append(")");
                return variablePattern.ToString();
            }

            bool treatAsList = listVariables.Contains(variable.Name);
            bool treatAsMap = mapVariables.Contains(variable.Name);

            bool considerString = !variable.Composite && !treatAsList && !treatAsMap;
            bool considerList = treatAsList || !treatAsMap;
            bool considerMap = treatAsMap || !treatAsList;

            variablePattern.Append("(?:");

            if (considerString)
            {
                // could be a simple string
                variablePattern.Append(nameStartPattern).Append(Regex.Escape(variable.Name)).Append(nameEndPattern).Append('=');
                variablePattern.Append(valueStartPattern);
                variablePattern.Append(characterPattern).Append(countPattern);
                variablePattern.Append(valueEndPattern);
            }

            if (considerList)
            {
                if (considerString)
                    variablePattern.Append('|');

                // could be an associative array
                variablePattern.Append(nameStartPattern).Append(Regex.Escape(variable.Name)).Append(nameEndPattern).Append('=');
                variablePattern.Append(valueStartPattern).Append(characterPattern).Append(countPattern).Append(valueEndPattern);
                if (!variable.Composite)
                {
                    /* Composite variables appear as separate query parameters that are aggregated by the Match method.
                     * This expression only needs to handle the non-composite case.
                     */
                    variablePattern.Append("(?:,");
                    variablePattern.Append(valueStartPattern).Append(characterPattern).Append(countPattern).Append(valueEndPattern);
                    variablePattern.Append(")*?");
                }
            }

            if (considerMap)
            {
                if (considerString || considerList)
                    variablePattern.Append('|');

                if (!variable.Composite)
                    variablePattern.Append(nameStartPattern).Append(Regex.Escape(variable.Name)).Append(nameEndPattern).Append('=');

                // could be an associative map
                char separator = variable.Composite ? '=' : ',';
                variablePattern.Append(valueStartPattern);
                variablePattern.Append(keyStartPattern);
                variablePattern.Append(characterPattern).Append(countPattern);
                variablePattern.Append(keyEndPattern);
                variablePattern.Append(separator).Append(mapValueStartPattern).Append(characterPattern).Append(countPattern).Append(mapValueEndPattern);
                variablePattern.Append(valueEndPattern);
                if (!variable.Composite)
                {
                    /* Composite variables appear as separate query parameters that are aggregated by the Match method.
                     * This expression only needs to handle the non-composite case.
                     */
                    variablePattern.Append("(?:,");
                    variablePattern.Append(valueStartPattern);
                    variablePattern.Append(keyStartPattern);
                    variablePattern.Append(characterPattern).Append(countPattern);
                    variablePattern.Append(keyEndPattern);
                    variablePattern.Append(separator).Append(mapValueStartPattern).Append(characterPattern).Append(countPattern).Append(mapValueEndPattern);
                    variablePattern.Append(valueEndPattern);
                    variablePattern.Append(")*?");
                }
            }

            variablePattern.Append(")");

            return variablePattern.ToString();
        }

        private static void AppendOneOrMoreUnorderedToEnd(StringBuilder pattern, List<string> patterns, int startIndex)
        {
            if (startIndex >= patterns.Count)
                throw new ArgumentException();

            StringBuilder anySinglePattern = new StringBuilder();
            anySinglePattern.Append("(?:");
            for (int i = 0; i < patterns.Count; i++)
            {
                if (i > 0)
                    anySinglePattern.Append("|");

                anySinglePattern.Append(patterns[i]);
            }

            anySinglePattern.Append(")");

            pattern.Append("(?:");
            pattern.Append(anySinglePattern);
            pattern.Append("(?:&");
            pattern.Append(anySinglePattern);
            pattern.Append(")*");
            pattern.Append(")");
        }

        protected internal override KeyValuePair<VariableReference, object>[] Match(string text, ICollection<string> listVariables, ICollection<string> mapVariables)
        {
#warning TODO: Check the cardinality of group var0name against the variable.Composite property.
            List<string> variablePatterns = new List<string>();
            for (int i = 0; i < Variables.Count; i++)
            {
                bool allowReservedSet = false;
                variablePatterns.Add(BuildVariablePattern(Variables[i], allowReservedSet, "var" + i, listVariables, mapVariables));
            }

            StringBuilder matchPattern = new StringBuilder();
            matchPattern.Append("^").Append(Regex.Escape(Type == UriTemplatePartType.QueryContinuation ? "&" : "?"));
            AppendOneOrMoreUnorderedToEnd(matchPattern, variablePatterns, 0);
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

                bool treatAsList = listVariables.Contains(Variables[i].Name);
                bool treatAsMap = mapVariables.Contains(Variables[i].Name);

                bool considerString = !Variables[i].Composite && !treatAsList && !treatAsMap;
                bool considerList = treatAsList || !treatAsMap;
                bool considerMap = treatAsMap || !treatAsList;

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

                // next try a list
                if (!considerString || group.Captures.Count > 1)
                {
                    Debug.Assert(considerList);
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
