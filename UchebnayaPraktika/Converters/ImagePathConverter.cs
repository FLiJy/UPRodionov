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
            string relativePath = value as string;
            if (string.IsNullOrEmpty(relativePath)) return null;

            // 1. Убираем ВСЕ начальные слэши, чтобы Path.Combine работал правильно
            string cleanPath = relativePath.TrimStart('/', '\\');

            // 2. Получаем правильный путь к папке программы
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, cleanPath);

            // 3. Проверяем, существует ли файл физически
            if (File.Exists(fullPath))
            {
                try
                {
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad; // Не блокируем файл
                    image.UriSource = new Uri(fullPath);
                    image.EndInit();
                    return image;
                }
                catch { return null; } // Защита от "битых" картинок
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}