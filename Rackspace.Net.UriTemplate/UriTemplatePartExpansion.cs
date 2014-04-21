// Copyright (c) Rackspace, US Inc. All Rights Reserved. Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Rackspace.Net
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using IDictionary = System.Collections.IDictionary;
    using IEnumerable = System.Collections.IEnumerable;

    /// <summary>
    /// This is the base class for <see cref="UriTemplatePart"/> instances which involve
    /// the expansion of variable references during rendering and/or matching.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    internal abstract class UriTemplatePartExpansion : UriTemplatePart
    {
        /// <summary>
        /// This regular expression pattern matches a single <c>unreserved</c> or <c>pct-encoded</c> character.
        /// </summary>
        /// <seealso href="http://tools.ietf.org/html/rfc6570#section-1.5">Notational Conventions (RFC 6570 URI Template)</seealso>
        protected const string UnreservedCharacterPattern = @"(?:[a-zA-Z0-9._~-]|" + UriTemplate.PctEncodedPattern + ")";

        /// <summary>
        /// This regular expression pattern matches a single <c>reserved</c> character.
        /// </summary>
        /// <seealso href="http://tools.ietf.org/html/rfc6570#section-1.5">Notational Conventions (RFC 6570 URI Template)</seealso>
        protected const string ReservedCharacterPattern = @"(?:[:/?#[\]@!$&'()*+,;=]" + @")";

        /// <summary>
        /// This is the backing field for the <see cref="Variables"/> property.
        /// </summary>
        private readonly VariableReference[] _variables;

        /// <summary>
        /// Initializes a new instance of the <see cref="UriTemplatePartExpansion"/> class
        /// with the specified variables.
        /// </summary>
        /// <param name="variables">A collection of <see cref="VariableReference"/> instances which describe the expansion variables referenced by this URI Template part.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="variables"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="variables"/> contains any <see langword="null"/> values.</exception>
        public UriTemplatePartExpansion(IEnumerable<VariableReference> variables)
        {
            if (variables == null)
                throw new ArgumentNullException("variables");

            _variables = variables.ToArray();
            if (_variables.Contains(null))
                throw new ArgumentException("variables cannot contain any null values", "variables");
        }

        /// <summary>
        /// Gets a collection of variables which are referenced by this expansion.
        /// </summary>
        /// <value>A read-only collection of <see cref="VariableReference"/> instances describing the variables referenced by this URI Template part.</value>
        public ReadOnlyCollection<VariableReference> Variables
        {
            get
            {
                return new ReadOnlyCollection<VariableReference>(_variables);
            }
        }

        /// <inheritdoc/>
        /// <remarks>
        /// The <see cref="Variables"/> are rendered in order. This method divides the rendering
        /// process into separate methods according to the type of parameter provided in
        /// <paramref name="parameters"/> for each of the variables in <see cref="Variables"/>.
        ///
        /// <list type="table">
        /// <listheader>
        /// <term>Parameter Type</term>
        /// <term>Render Action</term>
        /// </listheader>
        /// <item>
        /// <description><see cref="IDictionary"/></description>
        /// <description>The variable is rendered by calling <see cref="RenderDictionary"/>.</description>
        /// </item>
        /// <item>
        /// <description><see cref="IEnumerable"/> (except <see cref="String"/>)</description>
        /// <description>The variable is rendered by calling <see cref="RenderEnumerable"/>.</description>
        /// </item>
        /// <item>
        /// <description><see cref="String"/>, or any other <see cref="Object"/></description>
        /// <description>The variable is rendered by calling <see cref="RenderElement"/>.</description>
        /// </item>
        /// <item>
        /// <description><see langword="null"/></description>
        /// <description>The output is not modified.</description>
        /// </item>
        /// </list>
        /// </remarks>
        public override sealed void Render<T>(StringBuilder builder, IDictionary<string, T> parameters)
        {
            bool added = false;
            for (int i = 0; i < _variables.Length; i++)
            {
                T result;
                if (!parameters.TryGetValue(_variables[i].Name, out result) || result == null)
                    continue;

                IDictionary dictionary = result as IDictionary;
                if (dictionary != null)
                {
                    if (_variables[i].Prefix != null)
                        throw new InvalidOperationException(string.Format("Cannot apply prefix modifier to associative map value '{0}'", _variables[i].Name));

                    RenderDictionary(builder, _variables[i], dictionary, !added);
                }
                else
                {
                    IEnumerable enumerable = result as IEnumerable;
                    if (enumerable != null && !(result is string))
                    {
                        if (_variables[i].Prefix != null)
                            throw new InvalidOperationException(string.Format("Cannot apply prefix modifier to composite value '{0}'", _variables[i].Name));

                        RenderEnumerable(builder, _variables[i], enumerable, !added);
                    }
                    else
                    {
                        RenderElement(builder, _variables[i], result, !added);
                    }
                }

                added = true;
            }
        }

        /// <inheritdoc/>
        /// <remarks>
        /// This method checks that the definitions in <see cref="Variables"/> do not conflict with
        /// <paramref name="listVariables"/> and <paramref name="mapVariables"/>. It then calls
        /// <see cref="BuildPatternBodyImpl"/> to construct the actual pattern.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="listVariables"/> includes the name of a variable which specifies a <see cref="VariableReference.Prefix"/> in the template.
        /// <para>-or-</para>
        /// <para>If <paramref name="mapVariables"/> includes the name of a variable which specifies a <see cref="VariableReference.Prefix"/> in the template.</para>
        /// </exception>
        protected override sealed void BuildPatternBody(StringBuilder pattern, ICollection<string> listVariables, ICollection<string> mapVariables)
        {
            foreach (VariableReference variable in Variables)
            {
                if (variable.Prefix != null)
                {
                    if (listVariables.Contains(variable.Name))
                        throw new InvalidOperationException("Cannot treat a variable with a prefix modifier as a list.");
                    if (mapVariables.Contains(variable.Name))
                        throw new InvalidOperationException("Cannot treat a variable with a prefix modifier as an associative map.");
                }
            }

            BuildPatternBodyImpl(pattern, listVariables, mapVariables);
        }

        /// <summary>
        /// Provides the implementation of <see cref="BuildPatternBody"/> after variable constraints are
        /// checked against <paramref name="listVariables"/> and <paramref name="mapVariables"/>.
        /// </summary>
        /// <param name="pattern">The <see cref="StringBuilder"/> to append the regular expression pattern to.</param>
        /// <param name="listVariables">The names of variables which should be treated as lists during the match operation.</param>
        /// <param name="mapVariables">The names of variables which should be treated as associative maps during the match operation.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="pattern"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="listVariables"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="mapVariables"/> is <see langword="null"/>.</para>
        /// </exception>
        protected abstract void BuildPatternBodyImpl(StringBuilder pattern, ICollection<string> listVariables, ICollection<string> mapVariables);

        /// <summary>
        /// This helper method writes a string value to the <see cref="StringBuilder"/> output,
        /// percent-encoding characters and restricting the output according to the variable
        /// <see cref="VariableReference.Prefix"/> as necessary.
        /// </summary>
        /// <remarks>
        /// Characters which are not <c>unreserved</c> or <c>reserved</c> characters according
        /// to RFC 6570 are always percent-encoded when they are appended.
        /// </remarks>
        /// <param name="builder">The <see cref="StringBuilder"/> to append text to.</param>
        /// <param name="variable">The variable being rendered.</param>
        /// <param name="value">The string value of the variable being rendered.</param>
        /// <param name="escapeReserved"><see langword="true"/> to percent-encode <c>reserved</c> characters defined by RFC 6570; otherwise, <see langword="false"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="builder"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="variable"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="value"/> is <see langword="null"/>.</para>
        /// </exception>
        protected static void AppendText(StringBuilder builder, VariableReference variable, string value, bool escapeReserved)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");
            if (variable == null)
                throw new ArgumentNullException("variable");
            if (value == null)
                throw new ArgumentNullException("value");

            string text = value;
            if (variable.Prefix != null && text.Length > variable.Prefix)
                text = text.Substring(0, variable.Prefix.Value);

            text = EncodeReservedCharacters(text, !escapeReserved);
            builder.Append(text);
        }

        /// <summary>
        /// Render a single variable, where the variable value is an associative map (<see cref="IDictionary"/>).
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to render to.</param>
        /// <param name="variable">The variable being rendered.</param>
        /// <param name="variableValue">The value of the variable being rendered.</param>
        /// <param name="first">
        /// <see langword="true"/> if this is the first variable being rendered from this expression; otherwise,
        /// <see langword="false"/>. Variables which do not have an associated parameter, or whose parameter value
        /// is <see langword="null"/>, are treated as though they were completely omitted for the purpose of
        /// determining the first variable.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="builder"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="variable"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="variableValue"/> is <see langword="null"/>.</para>
        /// </exception>
        protected abstract void RenderDictionary(StringBuilder builder, VariableReference variable, IDictionary variableValue, bool first);

        /// <summary>
        /// Render a single variable, where the variable value is a collection (<see cref="IEnumerable"/>).
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to render to.</param>
        /// <param name="variable">The variable being rendered.</param>
        /// <param name="variableValue">The value of the variable being rendered.</param>
        /// <param name="first">
        /// <see langword="true"/> if this is the first variable being rendered from this expression; otherwise,
        /// <see langword="false"/>. Variables which do not have an associated parameter, or whose parameter value
        /// is <see langword="null"/>, are treated as though they were completely omitted for the purpose of
        /// determining the first variable.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="builder"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="variable"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="variableValue"/> is <see langword="null"/>.</para>
        /// </exception>
        protected abstract void RenderEnumerable(StringBuilder builder, VariableReference variable, IEnumerable variableValue, bool first);

        /// <summary>
        /// Render a single variable, where the variable value is a <see cref="String"/> or other single-valued
        /// element.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to render to.</param>
        /// <param name="variable">The variable being rendered.</param>
        /// <param name="variableValue">The value of the variable being rendered.</param>
        /// <param name="first">
        /// <see langword="true"/> if this is the first variable being rendered from this expression; otherwise,
        /// <see langword="false"/>. Variables which do not have an associated parameter, or whose parameter value
        /// is <see langword="null"/>, are treated as though they were completely omitted for the purpose of
        /// determining the first variable.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="builder"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="variable"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="variableValue"/> is <see langword="null"/>.</para>
        /// </exception>
        protected abstract void RenderElement(StringBuilder builder, VariableReference variable, object variableValue, bool first);
    }
}
