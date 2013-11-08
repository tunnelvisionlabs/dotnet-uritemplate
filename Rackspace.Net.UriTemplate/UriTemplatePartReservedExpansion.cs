namespace Rackspace.Net
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using DictionaryEntry = System.Collections.DictionaryEntry;
    using IDictionary = System.Collections.IDictionary;
    using IEnumerable = System.Collections.IEnumerable;

    internal sealed class UriTemplatePartReservedExpansion : UriTemplatePartExpansion
    {
        public UriTemplatePartReservedExpansion(IEnumerable<VariableReference> variables)
            : base(variables)
        {
        }

        public override UriTemplatePartType Type
        {
            get
            {
                return UriTemplatePartType.ReservedStringExpansion;
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
        }

        protected override void RenderDictionary(StringBuilder builder, VariableReference variable, IDictionary variableValue, bool first)
        {
            foreach (DictionaryEntry entry in variableValue)
            {
                if (variable.Composite)
                {
                    if (!first)
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
            return string.Format("{{+{0}}}", string.Join(",", Variables.Select(i => i.Name).ToArray()));
        }
    }
}
