using System.Threading.Tasks;

namespace EEDataGift
{
    class Program
    {
        /// <summary>
        /// automation of Gift Data flow on the EE My Account area
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="donorTelephone"></param>
        /// <param name="recipientTelephone"></param>
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
            var version = ChromeVersionProvider.GetChromeVersion(chromePath);
            await ChromeDriverDownloader.DownloadChromeDriver(version);

            await new EESelenium(username, password)
                .DataGift(donorTelephone, recipientTelephone, giftMb);
            return 0;
        }
    }
}
