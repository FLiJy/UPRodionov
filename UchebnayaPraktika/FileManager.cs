using System;
using System.IO;
using System.Windows;

namespace UchebnayaPraktika
{
    public static class FileManager
    {
        public static string SaveImage(string sourcePath, string folderName)
        {
            if (string.IsNullOrEmpty(sourcePath) || !File.Exists(sourcePath)) return null;

            try
            {
                // Путь к папке внутри bin/Debug/Images/folderName
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string targetDir = Path.Combine(baseDir, "Images", folderName);

                if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);

                // Генерируем уникальное имя, чтобы избежать совпадений
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(sourcePath);
                string destPath = Path.Combine(targetDir, fileName);

                File.Copy(sourcePath, destPath);

                // Возвращаем относительный путь для БД
                return $"/Images/{folderName}/{fileName}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message);
                return null;
            }
        }
    }
}