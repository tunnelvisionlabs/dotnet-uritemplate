// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace TunnelVisionLabs.Net
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
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
        protected UriTemplatePartExpansion(IEnumerable<VariableReference> variables)
        {
            if (variables == null)
                throw new ArgumentNullException("variables");

            _variables = new List<VariableReference>(variables).ToArray();
            foreach (VariableReference variable in _variables)
            {
                if (variable == null)
                    throw new ArgumentException("variables cannot contain any null values", "variables");
            }
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
        /// <para>The <see cref="Variables"/> are rendered in order. This method divides the rendering
        /// process into separate methods according to the type of parameter provided in
        /// <paramref name="parameters"/> for each of the variables in <see cref="Variables"/>.</para>
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
        /// <description><see cref="IEnumerable"/> (except <see cref="string"/>)</description>
        /// <description>The variable is rendered by calling <see cref="RenderEnumerable"/>.</description>
        /// </item>
        /// <item>
        /// <description><see cref="string"/>, or any other <see cref="object"/></description>
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
        /// <para>This method checks that the definitions in <see cref="Variables"/> do not conflict with
        /// <paramref name="arrayVariables"/> and <paramref name="mapVariables"/>. It then calls
        /// <see cref="BuildPatternBodyImpl"/> to construct the actual pattern.</para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// <para>If <paramref name="arrayVariables"/> includes the name of a variable which specifies a <see cref="VariableReference.Prefix"/> in the template.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="mapVariables"/> includes the name of a variable which specifies a <see cref="VariableReference.Prefix"/> in the template.</para>
        /// </exception>
        protected override sealed void BuildPatternBody(StringBuilder pattern, ICollection<string> requiredVariables, ICollection<string> arrayVariables, ICollection<string> mapVariables)
        {
            foreach (VariableReference variable in Variables)
            {
                if (variable.Prefix != null)
                {
                    if (arrayVariables.Contains(variable.Name))
                        throw new InvalidOperationException("Cannot treat a variable with a prefix modifier as an associative array.");
                    if (mapVariables.Contains(variable.Name))
                        throw new InvalidOperationException("Cannot treat a variable with a prefix modifier as an associative map.");
                }
            }

            BuildPatternBodyImpl(pattern, requiredVariables, arrayVariables, mapVariables);
        }

        /// <summary>
        /// Provides the implementation of <see cref="BuildPatternBody"/> after variable constraints are
        /// checked against <paramref name="arrayVariables"/> and <paramref name="mapVariables"/>.
        /// </summary>
        /// <param name="pattern">The <see cref="StringBuilder"/> to append the regular expression pattern to.</param>
        /// <param name="requiredVariables">A collection of variables which must be provided during the expansion process for the resulting URI to be valid.</param>
        /// <param name="arrayVariables">The names of variables which should be treated as associative arrays during the match operation.</param>
        /// <param name="mapVariables">The names of variables which should be treated as associative maps during the match operation.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>If <paramref name="pattern"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="requiredVariables"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="arrayVariables"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="mapVariables"/> is <see langword="null"/>.</para>
        /// </exception>
        protected abstract void BuildPatternBodyImpl(StringBuilder pattern, ICollection<string> requiredVariables, ICollection<string> arrayVariables, ICollection<string> mapVariables);

        /// <inheritdoc/>
        /// <remarks>
        /// <para>This method calls <see cref="MatchImpl"/> to perform the actual matching operation, and then
        /// checks that all variables present in both <see cref="Variables"/> and <paramref name="requiredVariables"/>
        /// have values assigned as part of the result.</para>
        /// </remarks>
        protected internal override sealed KeyValuePair<VariableReference, object>[] Match(string text, ICollection<string> requiredVariables, ICollection<string> arrayVariables, ICollection<string> mapVariables)
        {
            var result = MatchImpl(text, requiredVariables, arrayVariables, mapVariables);
            if (result == null)
                return null;

            if (requiredVariables.Count > 0)
            {
                foreach (var variable in Variables)
                {
                    if (!requiredVariables.Contains(variable.Name))
                        continue;

                    KeyValuePair<VariableReference, object> pair = default(KeyValuePair<VariableReference, object>);
                    foreach (var i in result)
                    {
                        if (i.Key == variable)
                        {
                            pair = i;
                            break;
                        }
                    }

                    if (pair.Key == null)
                        return null;

                    if (pair.Value == null)
                        return null;

                    // String implements IEnumerable, but empty strings count as a value
                    if (pair.Value is string)
                        continue;

                    if (pair.Value is IEnumerable && !((IEnumerable)pair.Value).GetEnumerator().MoveNext())
                        return null;
                }
            }

            return result;
        }

        /// <summary>
        /// Provides the implementation of <see cref="Match"/>.
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
        protected abstract KeyValuePair<VariableReference, object>[] MatchImpl(string text, ICollection<string> requiredVariables, ICollection<string> arrayVariables, ICollection<string> mapVariables);

        /// <summary>
        /// This helper method writes a string value to the <see cref="StringBuilder"/> output,
        /// percent-encoding characters and restricting the output according to the variable
        /// <see cref="VariableReference.Prefix"/> as necessary.
        /// </summary>
        /// <remarks>
        /// <para>Characters which are not <c>unreserved</c> or <c>reserved</c> characters according
        /// to RFC 6570 are always percent-encoded when they are appended.</para>
        /// </remarks>
        /// <param name="builder">The <see cref="StringBuilder"/> to append text to.</param>
        /// <param name="variable">The variable being rendered.</param>
        /// <param name="value">The string value of the variable being rendered.</param>
        /// <param name="escapeReserved"><see langword="true"/> to percent-encode <c>reserved</c> characters defined by RFC 6570; otherwise, <see langword="false"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>If <paramref name="builder"/> is <see langword="null"/>.</para>
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
        /// <para>If <paramref name="builder"/> is <see langword="null"/>.</para>
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
        /// <para>If <paramref name="builder"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="variable"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="variableValue"/> is <see langword="null"/>.</para>
        /// </exception>
        protected abstract void RenderEnumerable(StringBuilder builder, VariableReference variable, IEnumerable variableValue, bool first);

        /// <summary>
        /// Render a single variable, where the variable value is a <see cref="string"/> or other single-valued
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
        /// <para>If <paramref name="builder"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="variable"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="variableValue"/> is <see langword="null"/>.</para>
        /// </exception>
        protected abstract void RenderElement(StringBuilder builder, VariableReference variable, object variableValue, bool first);
    }
}
