namespace Rackspace.Net
{
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Represents a single part of a decomposed URI Template.
    /// </summary>
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
        /// <param name="builder">The <see cref="StringBuilder"/> to render this part to.</param>
        /// <param name="parameters">A collection of parameters for replacing variable references in the template.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="builder"/> is <see langword="null"/>.</exception>
        public abstract void Render<T>(StringBuilder builder, IDictionary<string, T> parameters)
            where T : class;

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

        protected static string EncodeReservedCharacters(string text, bool allowReservedSet)
        {
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
    }
}
