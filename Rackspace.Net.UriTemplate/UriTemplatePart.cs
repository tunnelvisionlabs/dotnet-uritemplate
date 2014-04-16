namespace Rackspace.Net
{
    using System.Collections.Generic;
    using System.Text;

    internal abstract class UriTemplatePart
    {
        public abstract UriTemplatePartType Type
        {
            get;
        }

        public abstract void Render(StringBuilder builder, IDictionary<string, object> parameters);

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
