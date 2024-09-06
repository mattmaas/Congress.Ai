using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace App.Converters
{
    public class HtmlToFormattedStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string text)
                return string.Empty;

            var stringBuilder = new StringBuilder();
            var regex = new Regex(@"<b>(.*?)</b>|([^<]+)", RegexOptions.Singleline);

            foreach (Match match in regex.Matches(text))
            {
                if (match.Groups[1].Success) // Bold text
                {
                    stringBuilder.Append($"<Bold>{match.Groups[1].Value}</Bold>");
                }
                else if (match.Groups[2].Success) // Normal text
                {
                    stringBuilder.Append(match.Groups[2].Value);
                }
            }

            return stringBuilder.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}