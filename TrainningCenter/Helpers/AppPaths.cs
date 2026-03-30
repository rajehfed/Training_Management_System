using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainningCenter.Helpers
{
    public static class AppPaths
    {
        public static readonly string PeopleImagesFolder =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 
                "TrainingCenter", 
                "PeopleImages");

        public static string keyPath = @"HKEY_CURRENT_USER\SOFTWARE\TrainningCenter";

        static AppPaths()
        {
            // Create the Folder if Does not exists
            if (!Directory.Exists(PeopleImagesFolder)) {
                Directory.CreateDirectory(PeopleImagesFolder);
                EventLogger.LogInfo($"Created directory: {PeopleImagesFolder}");
            }
        }
    }
}
