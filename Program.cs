using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace EEDataGift
{
    class Program
    {
        /// <summary>
        /// Automation of Gift Data flow on the EE My Account area (windows only!)
        /// </summary>
        /// <param name="username">Your EE username (usually your email address)</param>
        /// <param name="password">Your EE password</param>
        /// <param name="donorTelephone">e.g. 07700654321</param>
        /// <param name="recipientTelephone">e.g. 07700123456</param>
        /// <param name="giftMb">Megabytes to gift (Ensure you have enough!)</param>
        /// <param name="chromePath">Chrome path e.g. C:\chrome\chrome.exe or .\chrome.exe. Will search PATH if not provided.</param>
        /// <returns></returns>
        static async Task<int> Main(
            string username,
            string password,
            string donorTelephone,
            string recipientTelephone,
            int giftMb,
            string chromePath = null)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new Exception("Tool currently only supports Windows!");

            var version = ChromeVersionProvider.GetChromeVersion(chromePath);
            var chromeDriverPath = await ChromeDriverDownloader.DownloadChromeDriver(version);

            await new EESelenium(username, password, chromeDriverPath)
                .DataGift(donorTelephone, recipientTelephone, giftMb);
            return 0;
        }
    }
}
