// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace TunnelVisionLabs.Net
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
        /// <value>
        /// A <see cref="UriTemplatePartType"/> indicating the type of this part.
        /// </value>
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
        public abstract void Render<T>(StringBuilder builder, IDictionary<string, T> parameters);

        /// <summary>
        /// Build a regular expression pattern which matches this template part in a URI.
        /// </summary>
        /// <remarks>
        /// <para>This method delegates the construction of the actual pattern building operation to
        /// <see cref="BuildPatternBody"/>. The results of that call are then wrapped in a regular
        /// expression named capture group.</para>
        /// </remarks>
        /// <param name="pattern">The <see cref="StringBuilder"/> to append the pattern to.</param>
        /// <param name="groupName">The name to use for the named capture in the regular expression matching this template part.</param>
        /// <param name="requiredVariables">A collection of variables which must be provided during the expansion process for the resulting URI to be valid.</param>
        /// <param name="arrayVariables">A collection of variables to treat as associative arrays when matching a candidate URI to the template.</param>
        /// <param name="mapVariables">A collection of variables to treat as associative maps when matching a candidate URI to the template.</param>
        /// <exception cref="ArgumentException">
        /// <para>If <paramref name="pattern"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="groupName"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="requiredVariables"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="arrayVariables"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="mapVariables"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para>If <paramref name="groupName"/> is empty.</para>
        /// </exception>
        public void BuildPattern(StringBuilder pattern, string groupName, ICollection<string> requiredVariables, ICollection<string> arrayVariables, ICollection<string> mapVariables)
        {
            if (pattern == null)
                throw new ArgumentNullException("pattern");
            if (groupName == null)
                throw new ArgumentNullException("groupName");
            if (requiredVariables == null)
                throw new ArgumentNullException("requiredVariables");
            if (arrayVariables == null)
                throw new ArgumentNullException("arrayVariables");
            if (mapVariables == null)
                throw new ArgumentNullException("mapVariables");
            if (string.IsNullOrEmpty(groupName))
                throw new ArgumentException("groupName cannot be empty");

            pattern.Append("(?<").Append(groupName).Append('>');
            BuildPatternBody(pattern, requiredVariables, arrayVariables, mapVariables);
            pattern.Append(')');
        }

        /// <summary>
        /// Provides the implementation of <see cref="BuildPattern"/> for a specific <see cref="UriTemplatePart"/> type.
        /// </summary>
        /// <remarks>
        /// <para>This method is part of the <see cref="O:TunnelVisionLabs.Net.UriTemplate.Match"/> algorithm. If the match
        /// operation is successful, the text of the candidate URI matched by the segment of the regular expression
        /// added to <paramref name="pattern"/> by this method is passed as an argument to the <see cref="Match"/>
        /// method for associating the results with specific variables.</para>
        /// </remarks>
        /// <param name="pattern">The <see cref="StringBuilder"/> to append the pattern to.</param>
        /// <param name="requiredVariables">A collection of variables which must be provided during the expansion process for the resulting URI to be valid.</param>
        /// <param name="arrayVariables">A collection of variables to treat as associative arrays when matching a candidate URI to the template.</param>
        /// <param name="mapVariables">A collection of variables to treat as associative maps when matching a candidate URI to the template.</param>
        /// <exception cref="ArgumentException">
        /// <para>If <paramref name="pattern"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="requiredVariables"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="arrayVariables"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="mapVariables"/> is <see langword="null"/>.</para>
        /// </exception>
        protected abstract void BuildPatternBody(StringBuilder pattern, ICollection<string> requiredVariables, ICollection<string> arrayVariables, ICollection<string> mapVariables);

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

        /// <summary>
        /// Decodes text of a URI.
        /// </summary>
        /// <remarks>
        /// <para>The URI is assumed to be formed by first using the UTF-8 encoding to obtain
        /// a sequence of bytes, and then percent-encoding octets which are not allowed
        /// in the URI syntax. This method decodes text from a URI which was encoded by
        /// this process.</para>
        /// </remarks>
        /// <param name="text">The URI text.</param>
        /// <returns>The decoded URI text.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="text"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="text"/> contains a <c>%</c> character which is not part of an percent-encoded triplet.</exception>
        /// <exception cref="ArgumentException">If the byte sequence after decoding the percent-encoded triplets is not a valid UTF-8 byte sequence.</exception>
        protected static string DecodeCharacters(string text)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            byte[] sourceData = Encoding.UTF8.GetBytes(text);
            byte[] data = new byte[sourceData.Length];
            int length = 0;

            // the current position in sourceData
            int position = 0;

            // the index of the current % character in sourceData
            int index = -1;
            for (index = Array.IndexOf(sourceData, (byte)'%', index + 1); index >= 0; index = Array.IndexOf(sourceData, (byte)'%', index + 1))
            {
                while (position < index)
                {
                    data[length] = sourceData[position];
                    length++;
                    position++;
                }

                if (index > sourceData.Length - 3)
                    throw new ArgumentException("text contains a % character which is not part of a percent-encoded triplet");

                string hex = ((char)sourceData[index + 1]).ToString() + (char)sourceData[index + 2];
                byte value;
                if (!byte.TryParse(hex, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value))
                    throw new ArgumentException("text contains a % character which is not part of a percent-encoded triplet");

                data[length] = value;
                length++;
                position = index + 3;
            }

            while (position < sourceData.Length)
            {
                data[length] = sourceData[position];
                length++;
                position++;
            }

            return Encoding.UTF8.GetString(data, 0, length);
        }

        /// <summary>
        /// Implements the assignment of values to variables for the match operation.
        /// </summary>
        /// <param name="text">The text which was matched by the regular expression segment created by <see cref="BuildPatternBody"/>.</param>
        /// <param name="requiredVariables">A collection of variables which must be provided during the expansion process for the resulting URI to be valid.</param>
        /// <param name="arrayVariables">A collection of variables to treat as associative arrays when matching a candidate URI to the template.</param>
        /// <param name="mapVariables">A collection of variables to treat as associative maps when matching a candidate URI to the template.</param>
        /// <returns>
        /// <para>An array containing the assignment of values to variables for the current part.</para>
        /// <para>-or-</para>
        /// <para><see langword="null"/> if the matched <paramref name="text"/> does not provide a valid match for this template part.</para>
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <para>If <paramref name="text"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="requiredVariables"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="arrayVariables"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="mapVariables"/> is <see langword="null"/>.</para>
        /// </exception>
        protected internal abstract KeyValuePair<VariableReference, object>[] Match(string text, ICollection<string> requiredVariables, ICollection<string> arrayVariables, ICollection<string> mapVariables);
    }
}
