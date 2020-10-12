using System;
using System.Diagnostics;
using System.IO;

namespace EEDataGift
{
    static class ChromeVersionProvider
    {
        public static string GetChromeVersion(string providedChromePath)
        {
            string chromePath = null;
            if (File.Exists(providedChromePath))
            {
                chromePath = Path.GetFullPath(providedChromePath);

                Console.WriteLine($"Using chrome.exe path provided. {chromePath}");
            }
            else
            {
                Console.WriteLine($"chrome.exe did not exist at the path provided {providedChromePath}");
            }

            if (chromePath == null)
            {
                Console.WriteLine($"Searching PATH for chrome.exe...");

                chromePath = GetFullPath("chrome.exe");
            }

            if (chromePath == null)
            {
                throw new Exception("chrome.exe not found.");
            }

            var version = FileVersionInfo.GetVersionInfo(chromePath);
            return version.ProductVersion;
        }

        public static string GetFullPath(string fileName)
        {
            if (File.Exists(fileName))
                return Path.GetFullPath(fileName);

            var values = Environment.GetEnvironmentVariable("PATH");
            foreach (var path in values.Split(Path.PathSeparator))
            {
                var fullPath = Path.Combine(path, fileName);
                if (File.Exists(fullPath))
                    return fullPath;
            }

            return null;
        }
    }
}
