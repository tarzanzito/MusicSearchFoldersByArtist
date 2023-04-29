using System.Globalization;
using System.Text;


namespace MusicManager
{
    internal class Utils
    {
        internal enum Collection
        {
            MP3,
            FLAC
        }

        internal enum SearchType
        {
            CONSTany,   // name*
            anyCONSTany // *name*

        }

        internal enum TextCaseAction
        {
            None,
            ToLower,
            ToUpper
        }

        //remoção de chars diacríticos/// <summary>
        //stripping diacritics
        internal static string RemoveDiacritics(string text, TextCaseAction textCaseAction)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            string normalizedString = text.Normalize(NormalizationForm.FormD);

            StringBuilder stringBuilder = new StringBuilder(capacity: normalizedString.Length);

            foreach (char letter in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(letter);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(letter);
            }

            string result = stringBuilder.ToString().Normalize(NormalizationForm.FormC);
            
            //extra
            switch (textCaseAction)
            {
                case TextCaseAction.ToLower:
                    return result.ToLower();
                case TextCaseAction.ToUpper:
                    return result.ToUpper();
                default:
                    return result;
            }
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
