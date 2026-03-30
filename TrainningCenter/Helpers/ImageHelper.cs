using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace TrainningCenter.Helpers
{
    public static class ImageHelper
    {
        // Save the image inside the folder
        public static string SaveImage(string sourcePath)
        {
            if (string.IsNullOrEmpty(sourcePath) || !File.Exists(sourcePath))
                return null;

            string extension = Path.GetExtension(sourcePath);
            string newFileName = $"{Guid.NewGuid()}{extension}";
            string destinationPath = Path.Combine(AppPaths.PeopleImagesFolder, newFileName);

            try {
                File.Copy(sourcePath, destinationPath, true);
            }
            catch (Exception ex) {
                EventLogger.LogError("Error saving image", ex);
                return null;
            }

            return newFileName;
        }

        public static string UpdateImage(string oldImagePath, string newImagePath)
        {
            // Delete the old one
            DeleteImage(oldImagePath);

            return SaveImage(newImagePath);
        }

        public static void DeleteImage(string imagePath)
        {
            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                try {
                    File.Delete(imagePath);
                }
                catch (Exception ex) {
                    EventLogger.LogError("Error Deleting Image!!", ex);
                }
            }
        }

        public static string GetFullImagePath(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return null;

            return Path.Combine(AppPaths.PeopleImagesFolder, fileName);
        }
        public static BitmapImage LoadImage(string imagePath)
        {
            try
            {
                if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                    return new BitmapImage(new Uri(imagePath));

                // صورة افتراضية
                //return new BitmapImage(new Uri("/Assets/Images/default-user.png", UriKind.Relative));
            }
            catch
            {
                return new BitmapImage(new Uri("/Assets/Images/default-user.png", UriKind.Relative));
            }

            return new BitmapImage(new Uri(imagePath));
        }
    }
}
