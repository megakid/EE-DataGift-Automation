using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EEDataGift
{
    /// <summary>
    /// Main class to perform the selenium automation
    /// </summary>
    public class EESelenium
    {
        private readonly string _username;
        private readonly string _password;
        private readonly string _driverPath;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="driverPath"></param>
        public EESelenium(string username, string password, string driverPath = null)
        {
            _username = username ?? throw new ArgumentNullException(nameof(username));
            _password = password ?? throw new ArgumentNullException(nameof(password));
            _driverPath = driverPath;
        }

        internal async Task DataGift(
            string donorTelephone,
            string recipientTelephone,
            int mbToGift)
        {
            var from = CleanNumber(donorTelephone ?? throw new ArgumentNullException(nameof(donorTelephone)));
            var to = CleanNumber(recipientTelephone ?? throw new ArgumentNullException(nameof(recipientTelephone)));

            using IWebDriver driver = string.IsNullOrWhiteSpace(_driverPath)
                ? new ChromeDriver()
                : new ChromeDriver(ChromeDriverService.CreateDefaultService(_driverPath));

            try
            {
                driver.Url = "https://id.ee.co.uk/id/login";

                await Task.Delay(5000);

                // Cookie warning popup
                foreach (var btn in driver.FindElements(By.TagName("button")))
                {
                    if (btn.Text.Contains("accept", StringComparison.OrdinalIgnoreCase))
                        btn.Click();
                }
                await Task.Delay(50);

                // complete login
                driver.FindElement(By.Name("username")).SendKeys(_username);
                await Task.Delay(500);
                driver.FindElement(By.Name("password")).SendKeys(_password);
                await Task.Delay(500);

                driver.FindElement(By.Name("submitButton")).Click();
                await Task.Delay(3000);

                // navigate to family gifting page.
                driver.Navigate().GoToUrl("https://myaccount.ee.co.uk/app/family-gifting");
                await Task.Delay(3000);

                SelectElement selectDonor = null;
                SelectElement selectRecipient = null;

                // find both drop downs for donor and recepient phone numbers.
                var selects = driver.FindElements(By.TagName("select"));
                foreach (var sel in selects)
                {
                    if (sel.GetAttribute("form")?.Contains("giftDataForm", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        if (sel.GetAttribute("id")?.Contains("donor", StringComparison.OrdinalIgnoreCase) == true)
                        {
                            selectDonor = new SelectElement(sel);
                        }
                        else if (sel.GetAttribute("id")?.Contains("recipient", StringComparison.OrdinalIgnoreCase) == true)
                        {
                            selectRecipient = new SelectElement(sel);
                        }
                    }
                }

                if (selectDonor == null || selectRecipient == null)
                    throw new Exception("Couldn't find donor and recipient <select> dropdown elements");

                IWebElement form = null;
                IWebElement mbLabel = null;
                IWebElement incrementButton = null;
                IWebElement giftButton = null;

                // These take a while to populate so keep trying...
                const int maxAttempts = 10;
                var attempts = 0;
                while (true)
                {
                    try
                    {
                        // Select the donor in the donor drop down
                        foreach (var opt in selectDonor.Options)
                        {
                            if (CleanNumber(opt.Text).Contains(from, StringComparison.OrdinalIgnoreCase))
                            {
                                selectDonor.SelectByText(opt.Text);
                                break;
                            }
                        }
                        await Task.Delay(50);
                        // Select the recepient in the recepient drop down
                        foreach (var opt in selectRecipient.Options)
                        {
                            if (CleanNumber(opt.Text).Contains(to, StringComparison.OrdinalIgnoreCase))
                            {
                                selectRecipient.SelectByText(opt.Text);
                                break;
                            }
                        }
                        await Task.Delay(75);


                        // locate the entire giftDataForm
                        form = driver
                            .FindElement(By.Id("giftDataForm"));

                        // Search for the amount (MB/GB) display span
                        mbLabel = form.FindElements(By.TagName("span"))
                            .Single(s => s.GetAttribute("class")?.Contains("giftingDisplayAmount", StringComparison.OrdinalIgnoreCase) == true);

                        // Search for the increment button
                        incrementButton = form.FindElements(By.TagName("button"))
                            .Single(s => s.GetAttribute("class")?.Contains("ee-icon-plus", StringComparison.OrdinalIgnoreCase) == true);

                        // Now find and click the "Gift" button
                        giftButton = form.FindElements(By.TagName("button"))
                            .Single(s => s.GetAttribute("class")?.Contains("gift", StringComparison.OrdinalIgnoreCase) == true
                                && s.Text?.Contains("gift", StringComparison.OrdinalIgnoreCase) == true);

                        break;
                    }
                    catch
                    {
                        if (attempts++ > maxAttempts)
                            throw;
                        else
                            await Task.Delay(1000);
                    }
                }

                // While the display shows less than we want to gift, click the increment button
                var currentGift = ParseMegabytes(mbLabel.Text);
                while (currentGift < mbToGift)
                {
                    incrementButton.Click();
                    await Task.Delay(50);

                    currentGift = ParseMegabytes(mbLabel.Text);
                }

                Console.WriteLine($"Gifting {currentGift}MB from 0{from} to 0{to}.");

                giftButton.Click();

                await Task.Delay(50);

                // Now find and click the "Yes" confirmation button
                var confirmButton = form.FindElements(By.TagName("button"))
                    .Single(s => s.GetAttribute("type")?.Contains("submit", StringComparison.OrdinalIgnoreCase) == true
                        && s.Text?.Contains("yes", StringComparison.OrdinalIgnoreCase) == true);

                confirmButton.Click();

                Console.WriteLine($"Gifted {currentGift}MB from 0{from} to 0{to}.");

                await Task.Delay(2000);
            }
            finally
            {
                await Task.Delay(500);
                driver.Quit();
            }

            decimal ParseMegabytes(string text)
            {
                if (text.Contains("MB"))
                {
                    return decimal.Parse(text.Replace("MB", "").Trim());
                }
                else if (text.Contains("GB"))
                {
                    return decimal.Parse(text.Replace("GB", "").Trim()) * 1000;
                }

                throw new InvalidOperationException($"Cannot parse text {text}");
            }

            string CleanNumber(string num)
                => num?.Replace(" ", "")?.TrimStart('0');
        }
    }
}
