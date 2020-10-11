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
        /// <param name="giftMb"></param>
        /// <returns></returns>
        static async Task<int> Main(
            string username,
            string password,
            string donorTelephone,
            string recipientTelephone,
            int giftMb)
        {
            await new EESelenium(username, password)
                .DataGift(donorTelephone, recipientTelephone, giftMb);
            return 0;
        }
    }
}
