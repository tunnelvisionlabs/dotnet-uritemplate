// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace TunnelVisionLabs.Net
{
    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Represents a variable reference within a <see cref="UriTemplate"/> expression.
    /// </summary>
    /// <threadsafety static="true" instance="true"/>
    /// <preliminary/>
    public class VariableReference
    {
        /// <summary>
        /// This regular expression is used to validate the <see cref="Name"/> property when
        /// a <see cref="VariableReference"/> is initialized.
        /// </summary>
        private static readonly Regex NameExpression = new Regex(@"^" + UriTemplate.VarNamePattern + @"$", InternalRegexOptions.Default);

        /// <summary>
        /// This is the backing field for the <see cref="Name"/> property.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// This is the backing field for the <see cref="Prefix"/> property.
        /// </summary>
        private readonly int? _prefix;

        /// <summary>
        /// This is the backing field for the <see cref="Composite"/> property.
        /// </summary>
        private readonly bool _composite;

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableReference"/> class with the specified name.
        /// </summary>
        /// <param name="name">The name of the referenced variable.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        /// <exception cref="FormatException">If <paramref name="name"/> is not a valid <c>varname</c> according to RFC 6570.</exception>
        internal VariableReference(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty", "name");
            if (!NameExpression.IsMatch(name))
                throw new FormatException("name must match the format described in RFC 6570");

            _name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableReference"/> class with the specified name and prefix length.
        /// </summary>
        /// <param name="name">The name of the referenced variable.</param>
        /// <param name="prefix">The number of Unicode code points from the referenced variable expansion to include in the expanded URI.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="prefix"/> is less than or equal to 0, or greater than 9999.</exception>
        /// <exception cref="FormatException">If <paramref name="name"/> is not a valid <c>varname</c> according to RFC 6570.</exception>
        internal VariableReference(string name, int prefix)
            : this(name)
        {
            if (prefix <= 0)
                throw new ArgumentOutOfRangeException("prefix cannot be less than or equal to 0", "prefix");
            if (prefix > 9999)
                throw new ArgumentOutOfRangeException("prefix cannot be greater than 9999", "prefix");

            _prefix = prefix;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableReference"/> class with the specified name and composite modifier.
        /// </summary>
        /// <param name="name">The name of the referenced variable.</param>
        /// <param name="composite"><see langword="true"/> if this is a composite variable reference; otherwise, <see langword="false"/>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        /// <exception cref="FormatException">If <paramref name="name"/> is not a valid <c>varname</c> according to RFC 6570.</exception>
        internal VariableReference(string name, bool composite)
            : this(name)
        {
            _composite = composite;
        }

        /// <summary>
        /// Gets the name of the referenced template variable.
        /// </summary>
        /// <value>
        /// The name of the referenced template variable.
        /// </value>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Gets the number of Unicode code points to include from the variable
        /// expansion in the expanded URI, if this is a prefix variable reference
        /// (i.e. the <c>:</c> operator was used).
        /// </summary>
        /// <value>
        /// <para>The number of Unicode code points from the referenced variable expansion
        /// to include in the expanded URI.</para>
        /// <para>-or-</para>
        /// <para><see langword="null"/> if the number of code points for the expansion is not limited.</para>
        /// </value>
        public int? Prefix
        {
            get
            {
                return _prefix;
            }
        }

        /// <summary>
        /// Gets a value indicating whether or not this is a composite variable reference.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this is a composite variable reference (i.e. the explode modified <c>*</c> was used in the template); otherwise, <see langword="false"/>.
        /// </value>
        public bool Composite
        {
            get
            {
                return _composite;
            }
        }

        /// <summary>
        /// Parse a variable reference from an RFC 6570 URI Template to a <see cref="VariableReference"/> object.
        /// </summary>
        /// <param name="variable">The RFC 6570 variable reference to parse.</param>
        /// <returns>A <see cref="VariableReference"/> object representing the variable reference.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="variable"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="variable"/> is empty.</exception>
        /// <exception cref="FormatException">If <paramref name="variable"/> is not a valid <c>varspec</c> according to RFC 6570.</exception>
        internal static VariableReference Parse(string variable)
        {
            if (variable == null)
                throw new ArgumentNullException("reference");
            if (string.IsNullOrEmpty(variable))
                throw new ArgumentException("reference cannot be empty");

            try
            {
                bool explode = variable[variable.Length - 1] == '*';
                if (explode)
                {
                    string name = variable.Substring(0, variable.Length - 1);
                    return new VariableReference(name, true);
                }

                int colon = variable.IndexOf(':');
                if (colon >= 0)
                {
                    string name = variable.Substring(0, colon);
                    int prefix = int.Parse(variable.Substring(colon + 1));
                    return new VariableReference(name, prefix);
                }

                return new VariableReference(variable);
            }
            catch (Exception ex)
            {
                throw new FormatException("The specified varspec is not valid.", ex);
            }
        }

        /// <summary>
        /// Returns a string representation of the variable reference, in the format described in RFC 6570.
        /// </summary>
        /// <returns>
        /// A string representation of the variable reference, in the format described in RFC 6570.
        /// </returns>
        public override string ToString()
        {
            if (_prefix.HasValue)
                return string.Format("{0}:{1}", _name, _prefix);

            if (_composite)
                return string.Format("{0}*", _name);

            return _name;
        }
    }
}
