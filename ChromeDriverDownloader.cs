using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace EEDataGift
{
    static class ChromeDriverDownloader
    {
        public static async Task<string> DownloadChromeDriver(string chromeVersion)
        {
            var majorVersion = chromeVersion.Split('.').First();
            Console.WriteLine($"chrome.exe version {chromeVersion}, major version is {majorVersion}.");

            using var httpClient = new HttpClient();
            var latestVersion = (await httpClient.GetStringAsync("http://chromedriver.storage.googleapis.com/LATEST_RELEASE_86")).Trim();

            Console.WriteLine($"Latest chromedriver.exe compatible with {majorVersion} is {latestVersion}.");

            if (File.Exists("chromedriver.exe"))
                File.Delete("chromedriver.exe");

            Console.WriteLine($"Downloading chromedriver.exe {latestVersion}...");
            const string ChromeDriverDistroFileName = "chromedriver_win32.zip";
            using (var fs = File.Create(ChromeDriverDistroFileName))
            {
                using (var stream = await httpClient.GetStreamAsync($"http://chromedriver.storage.googleapis.com/{latestVersion}/{ChromeDriverDistroFileName}"))
                {
                    await stream.CopyToAsync(fs);
                    await fs.FlushAsync();
                }
            }

            ZipFile.ExtractToDirectory(ChromeDriverDistroFileName, ".");

            Console.WriteLine($"Unzipped {latestVersion} of chromedriver.exe to working directory.");

            return Path.GetFullPath("chromedriver.exe");
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
