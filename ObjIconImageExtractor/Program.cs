using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace ObjIconImageExtractor
{
    internal class Program
    {
        public const string dodDownloadsObjIconsDirectory = @"C:\Program Files (x86)\Steam\steamapps\common\Half-Life\dod_downloads\sprites\obj_icons";
        public const string dodObjIconsDirectoy = @"C:\Program Files (x86)\Steam\steamapps\common\Half-Life\dod\sprites\obj_icons";
        public const string outputDirectory = @"C:\temp\sprites_output";
        public const string allBmpsOutputDirectory = @"C:\temp\all_bmps";
        public const string pathToDecompmdl = @"C:\Users\Bill\source\repos\obj_icon_image_extractor\ObjIconImageExtractor\decompmdl\";
        public const string decompmdlFilename = @"decompmdl.exe";  // Source code and file download for this exe: https://github.com/Toodles2You/halflife-tools
        public const string decompmdlParameters = " \"{0}\" \"{1}\"";

        static void Main(string[] args)
        {
            string[] files = Directory.GetFiles(dodObjIconsDirectoy, "*.spr", SearchOption.AllDirectories);
            string[] downloadsFiles = Directory.GetFiles(dodDownloadsObjIconsDirectory, "*.spr", SearchOption.AllDirectories);

            string[] allObjIcons = files.Concat(downloadsFiles).ToArray();

            foreach (string filePath in allObjIcons)
            {
                string parentFolder = Path.GetFileName(Path.GetDirectoryName(filePath));
                string outputPathWithMapName = Path.Combine(outputDirectory, parentFolder);
                Directory.CreateDirectory(outputPathWithMapName);

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        // Source code and file download for this exe: https://github.com/Toodles2You/halflife-tools
                        FileName = Path.Combine(pathToDecompmdl, decompmdlFilename),
                        Arguments = String.Format(decompmdlParameters, filePath, outputPathWithMapName),
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                        ErrorDialog = false,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        WorkingDirectory = pathToDecompmdl
                    }
                };

                StringBuilder rawResGenOutput = new StringBuilder();
                try
                {
                    bool startedWithoutError = process.Start();

                    if (!startedWithoutError)
                    {
                        Console.WriteLine("Error starting decompmdl.exe");
                        break;
                    }

                    process.WaitForExit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error starting decompmdl.exe: ", ex.Message);
                    break;
                }
            }


            string[] bmpFiles = Directory.GetFiles(outputDirectory, "*.bmp", SearchOption.AllDirectories);

            foreach (string filePath in bmpFiles)
            {
                // Walk back up 3 parent directories to get map name
                DirectoryInfo fileDir = new FileInfo(filePath).Directory;
                string prefix = "";

                for (int i = 0; i < 3 && fileDir != null; i++)
                {
                    prefix = fileDir.Name;
                    fileDir = fileDir.Parent;
                }

                string originalFileName = Path.GetFileName(filePath);

                // Create new file name with map name or folder as a prefix (some maps put their icons right in obj_icons instead of a sub-folder)
                string newFileName = $"{prefix}_{originalFileName}";
                string destinationPath = Path.Combine(allBmpsOutputDirectory, newFileName);

                File.Copy(filePath, destinationPath, overwrite: true);
            }
        }
    }
}
