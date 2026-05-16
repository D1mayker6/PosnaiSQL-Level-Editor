using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace PosnaiSQLauncher.Services
{
    public static class ImageService
    {
        /// <summary>
        /// Конвертирует файл изображения в Base64
        /// </summary>
        public static string ImageToBase64(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
                return null;

            try
            {
                byte[] imageBytes = File.ReadAllBytes(imagePath);
                return Convert.ToBase64String(imageBytes);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Конвертирует Base64 в BitmapImage для WPF
        /// </summary>
        public static BitmapImage Base64ToImage(string base64String)
        {
            if (string.IsNullOrEmpty(base64String))
                return null;

            try
            {
                byte[] imageBytes = Convert.FromBase64String(base64String);
                
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = new MemoryStream(imageBytes);
                bitmap.EndInit();
                bitmap.Freeze();
                
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Получает размер файла в KB
        /// </summary>
        public static double GetFileSizeInKB(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
                return 0;

            var fileInfo = new FileInfo(imagePath);
            return fileInfo.Length / 1024.0;
        }
    }
}