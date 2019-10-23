using System;
using System.Globalization;
using System.Windows.Data;

namespace RocketLaunch.Helper
{
    public class StringToRichTextBoxConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            return true; // Do the conversion from bool to visibility
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
            //return new object[] { };
        }
    }
}