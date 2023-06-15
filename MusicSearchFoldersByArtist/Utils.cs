using System.Globalization;
using System.Text;


namespace MusicManager
{
    internal class Utils
    {
        internal enum SearchType
        {
            CONSTany,   // name*
            anyCONSTany // *name*

        }
        internal static string GetFirstLetter(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            return text.Substring(0, 1);

        }

        internal static string AddasteriskAtEnd(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            string lastChar = text.Substring((text.Length - 1), 1);
            if (lastChar == "*")
                return text;

            return text + "*";
        }

    }
}
