using System;
using System.Globalization;
using System.Windows.Data;

namespace ERGLauncher.Converters
{
    /// <summary>
    /// Convert <see cref="System.Enum"/> to <see cref="string"/>.
    /// </summary>
    public class EnumToBooleanConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return false;

            return value.ToString() == parameter.ToString();
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return Binding.DoNothing;

            if ((bool)value)
            {
                return Enum.Parse(targetType, parameter.ToString()!);
            }

            return Binding.DoNothing;
        }
    }
}
