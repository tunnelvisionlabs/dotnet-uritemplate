// Copyright (c) Rackspace, US Inc. All Rights Reserved. Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Rackspace.Net
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;

    /// <summary>
    /// Represents a single part of a decomposed URI Template.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    internal abstract class UriTemplatePart
    {
        /// <summary>
        /// Gets the type of this part.
        /// </summary>
        public abstract UriTemplatePartType Type
        {
            get;
        }

        /// <summary>
        /// Renders this <see cref="UriTemplatePart"/> to a <see cref="StringBuilder"/>, applying the
        /// specified <paramref name="parameters"/> as replacements for variables in the template.
        /// </summary>
        /// <typeparam name="T">The type of parameter value provided in <paramref name="parameters"/>.</typeparam>
        /// <param name="builder">The <see cref="StringBuilder"/> to render this part to.</param>
        /// <param name="parameters">A collection of parameters for replacing variable references in the template.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="builder"/> is <see langword="null"/>.</exception>
        public abstract void Render<T>(StringBuilder builder, IDictionary<string, T> parameters)
            where T : class;

        /// <summary>
        /// Build a regular expression pattern which matches this template part in a URI.
        /// </summary>
        /// <remarks>
        /// This method delegates the construction of the actual pattern building operation to
        /// <see cref="BuildPatternBody"/>. The results of that call are then wrapped in a regular
        /// expression named capture group.
        /// </remarks>
        /// <param name="pattern">The <see cref="StringBuilder"/> to append the pattern to.</param>
        /// <param name="groupName">The name to use for the named capture in the regular expression matching this template part.</param>
        /// <param name="listVariables">A collection of variables to treat as lists when matching a candidate URI to the template.</param>
        /// <param name="mapVariables">A collection of variables to treat as associative maps when matching a candidate URI to the template.</param>
        /// <exception cref="ArgumentException">
        /// If <paramref name="pattern"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="groupName"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="listVariables"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="mapVariables"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="groupName"/> is empty.
        /// </exception>
        public void BuildPattern(StringBuilder pattern, string groupName, ICollection<string> listVariables, ICollection<string> mapVariables)
        {
            if (pattern == null)
                throw new ArgumentNullException("pattern");
            if (groupName == null)
                throw new ArgumentNullException("groupName");
            if (listVariables == null)
                throw new ArgumentNullException("listVariables");
            if (mapVariables == null)
                throw new ArgumentNullException("mapVariables");
            if (string.IsNullOrEmpty(groupName))
                throw new ArgumentException("groupName cannot be empty");

            pattern.Append("(?<").Append(groupName).Append('>');
            BuildPatternBody(pattern, listVariables, mapVariables);
            pattern.Append(')');
        }

        /// <summary>
        /// Provides the implementation of <see cref="BuildPattern"/> for a specific <see cref="UriTemplatePart"/> type.
        /// </summary>
        /// <remarks>
        /// This method is part of the <see cref="UriTemplate.Match"/> algorithm. If the match operation is
        /// successful, the text of the candidate URI matched by the segment of the regular expression added
        /// to <paramref name="pattern"/> by this method is passed as an argument to the <see cref="Match"/>
        /// method for associating the results with specific variables.
        /// </remarks>
        /// <param name="pattern">The <see cref="StringBuilder"/> to append the pattern to.</param>
        /// <param name="listVariables">A collection of variables to treat as lists when matching a candidate URI to the template.</param>
        /// <param name="mapVariables">A collection of variables to treat as associative maps when matching a candidate URI to the template.</param>
        /// <exception cref="ArgumentException">
        /// If <paramref name="pattern"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="listVariables"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="mapVariables"/> is <see langword="null"/>.</para>
        /// </exception>
        protected abstract void BuildPatternBody(StringBuilder pattern, ICollection<string> listVariables, ICollection<string> mapVariables);

        /// <summary>
        /// Determines if an ASCII character matches the <c>unreserved</c> pattern defined
        /// in RFC 6570.
        /// </summary>
        /// <param name="b">The ASCII character to test.</param>
        /// <returns><see langword="true"/> if <paramref name="b"/> is an <c>unreserved</c> character; otherwise, <see langword="false"/>.</returns>
        protected static bool IsUnreserved(byte b)
        {
            if (b >= 'a' && b <= 'z')
                return true;

            if (b >= 'A' && b <= 'Z')
                return true;

            if (b >= '0' && b <= '9')
                return true;

            switch ((char)b)
            {
            case '-':
            case '.':
            case '_':
            case '~':
                return true;

            default:
                return false;
            }
        }

        /// <summary>
        /// Determines if an ASCII character matches the <c>reserved</c> pattern defined
        /// in RFC 6570.
        /// </summary>
        /// <param name="b">The ASCII character to test.</param>
        /// <returns><see langword="true"/> if <paramref name="b"/> is a <c>reserved</c> character; otherwise, <see langword="false"/>.</returns>
        protected static bool IsReserved(byte b)
        {
            switch ((char)b)
            {
            // gen-delims
            case ':':
            case '/':
            case '?':
            case '#':
            case '[':
            case ']':
            case '@':
                return true;

            // sub-delims
            case '!':
            case '$':
            case '&':
            case '\'':
            case '(':
            case ')':
            case '*':
            case '+':
            case ',':
            case ';':
            case '=':
                return true;

            default:
                return false;
            }
        }

        /// <summary>
        /// Encodes text for inclusion in a URI via an expansion.
        /// </summary>
        /// <param name="text">The text to encode.</param>
        /// <param name="allowReservedSet"><see langword="true"/> to allow <c>reserved</c> characters to pass through without percent-encoding; otherwise, <see langword="false"/> to percent-encode these characters.</param>
        /// <returns>The encoded text for inclusion in a URI.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="text"/> is <see langword="null"/>.</exception>
        protected static string EncodeReservedCharacters(string text, bool allowReservedSet)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            StringBuilder builder = new StringBuilder();
            byte[] encoded = Encoding.UTF8.GetBytes(text);
            foreach (byte b in encoded)
            {
                bool escape = !IsUnreserved(b);
                if (escape && allowReservedSet && IsReserved(b))
                    escape = false;

                if (escape)
                    builder.Append('%').Append(b.ToString("X2"));
                else
                    builder.Append((char)b);
            }

            return builder.ToString();
        }

        protected static string DecodeCharacters(string text)
        {
            byte[] data = new byte[text.Length];
            int length = 0;

            // the current position in text
            int position = 0;
            // the index of the current % character in text
            int index = -1;
            for (index = text.IndexOf('%', index + 1); index >= 0; index = text.IndexOf('%', index + 1))
            {
                while (position < index)
                {
                    data[length] = (byte)text[position];
                    length++;
                    position++;
                }

                string hex = text.Substring(index + 1, 2);
                int value = int.Parse(hex, NumberStyles.AllowHexSpecifier);
                data[length] = (byte)value;
                length++;
                position = index + 3;
            }

            while (position < text.Length)
            {
                data[length] = (byte)text[position];
                length++;
                position++;
            }

            return Encoding.UTF8.GetString(data, 0, length);
        }

        /// <summary>
        /// Implements the assignment of values to variables for the match operation.
        /// </summary>
        /// <param name="text">The text which was matched by the regular expression segment created by <see cref="BuildPatternBody"/>.</param>
        /// <param name="listVariables">A collection of variables to treat as lists when matching a candidate URI to the template.</param>
        /// <param name="mapVariables">A collection of variables to treat as associative maps when matching a candidate URI to the template.</param>
        /// <returns>An array containing the assignment of values to variables for the current part.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="text"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="listVariables"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="mapVariables"/> is <see langword="null"/>.</para>
        /// </exception>
        protected internal abstract KeyValuePair<VariableReference, object>[] Match(string text, ICollection<string> listVariables, ICollection<string> mapVariables);
    }
}
