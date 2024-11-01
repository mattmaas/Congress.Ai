using System.Globalization;
using System.Text.RegularExpressions;

namespace App.Converters
{
    public class HtmlToFormattedStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string text)
                return new FormattedString();

            var formattedString = new FormattedString();
            var regex = new Regex(@"<b>(.*?)</b>|([^<]+)", RegexOptions.Singleline);

            foreach (Match match in regex.Matches(text))
            {
                if (match.Groups[1].Success) // Bold text
                {
                    formattedString.Spans.Add(new Span { Text = match.Groups[1].Value, FontAttributes = FontAttributes.Bold });
                }
                else if (match.Groups[2].Success) // Normal text
                {
                    formattedString.Spans.Add(new Span { Text = match.Groups[2].Value });
                }
            }

            return formattedString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
