using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace UchebnayaPraktika.Converters
{
    public class ImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            string path = value.ToString().TrimStart('/', '\\');
            // Склеиваем путь с ПАПКОЙ ЗАПУСКА приложения
            string fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);

            if (System.IO.File.Exists(fullPath))
            {
                return new BitmapImage(new Uri(fullPath));
            }
            return null; // Если файла нет физически по этому адресу
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}