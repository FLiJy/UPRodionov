using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace UchebnayaPraktika.Converters // <-- Добавили .Converters
{
    public class FreezeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isFrozen)
                return isFrozen ? "❄ Заморожена" : "✅ Активна";
            return "Неизвестно";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    }

    public class FreezeColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isFrozen)
                return isFrozen ? Brushes.IndianRed : Brushes.DarkGreen;
            return Brushes.Black;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    }

    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isVisible = false;

            if (value is bool b)
            {
                isVisible = b;
            }

            // Если из XAML передали параметр "invert", меняем логику на противоположную
            if (parameter != null && parameter.ToString() == "invert")
            {
                isVisible = !isVisible;
            }

            // Обязательно возвращаем строгий тип Visibility, иначе XDG-000 будет ругаться
            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    }
}