using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using NUnit.Framework;
using System;
using System.Configuration;
using System.IO;
using System.Threading;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using WebDriverManager.DriverConfigs.Impl;
using NUnit.Framework.Interfaces;
using Allure.Commons;
using Status = Allure.Commons.Status;

namespace roomstogoseleniumframework.Utilities
{
    public class Base
    {
        public ThreadLocal<IWebDriver> driver = new();
        protected WebDriverWait wait;
        private static ExtentReports extent;
        protected ExtentTest test;

        

        [SetUp]
        public void StartBrowser()
        {
            // Start the ExtentTest for the current test
            test = extent.CreateTest(TestContext.CurrentContext.Test.Name);

            string browserName = TestContext.Parameters["browserName"];
            if (string.IsNullOrEmpty(browserName))
            {
                browserName = ConfigurationManager.AppSettings["browser"];
            }

            string baseUrl = ConfigurationManager.AppSettings["baseUrl"];

            if (string.IsNullOrEmpty(browserName))
            {
                throw new ArgumentNullException("Browser name is not specified in App.config");
            }

            InitBrowser(browserName);

            if (driver.Value != null)
            {
                driver.Value.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(20);
                driver.Value.Manage().Window.Maximize();
                driver.Value.Navigate().GoToUrl(baseUrl);
                

                wait = new WebDriverWait(driver.Value, TimeSpan.FromSeconds(20));
            }
        }

        public IWebDriver GetDriver() => driver.Value;

        public void InitBrowser(string browserName)
        {
            switch (browserName)
            {
                case "Firefox":
                    new WebDriverManager.DriverManager().SetUpDriver(new FirefoxConfig());
                    driver.Value = new FirefoxDriver();
                    break;
                case "Chrome":
                    var chromeOptions = new ChromeOptions();
                    var chromeDriverService = ChromeDriverService.CreateDefaultService();
                    driver.Value = new ChromeDriver(chromeDriverService, chromeOptions, TimeSpan.FromMinutes(3));
                    break;
                case "Edge":
                    var edgeOptions = new EdgeOptions();
                    var edgeDriverService = EdgeDriverService.CreateDefaultService();
                    driver.Value = new EdgeDriver(edgeDriverService, edgeOptions, TimeSpan.FromMinutes(3));
                    break;
                default:
                    throw new ArgumentException("Browser name is not recognized.");
            }

            if (driver.Value == null)
            {
                throw new InvalidOperationException("WebDriver initialization failed.");
            }
        }

        public void CloseAnyPopupOrDialog()
        {
            RetryAction(CloseModalPopup);
            RetryAction(HandleAlerts);
            RetryAction(CloseCookieConsent);
            RetryAction(CloseAbtIconPopup);
            RetryAction(CloseBannerPopup);
        }

        private void CloseModalPopup()
        {
            try
            {
                IWebElement closeModalButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.CssSelector(".close-modal-selector")));
                closeModalButton.Click();
            }
            catch (WebDriverTimeoutException) { }
            catch (NoSuchElementException) { }
        }

        private void HandleAlerts()
        {
            try
            {
                IAlert alert = driver.Value.SwitchTo().Alert();
                alert.Accept();
            }
            catch (NoAlertPresentException) { }
        }

        private void CloseCookieConsent()
        {
            try
            {
                IWebElement cookieConsentCloseButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.CssSelector(".onetrust-close-btn-handler")));
                try
                {
                    cookieConsentCloseButton.Click();
                }
                catch (ElementClickInterceptedException)
                {
                    ((IJavaScriptExecutor)driver.Value).ExecuteScript("arguments[0].click();", cookieConsentCloseButton);
                }
            }
            catch (WebDriverTimeoutException) { }
            catch (NoSuchElementException) { }
        }

        private void CloseAbtIconPopup()
        {
            try
            {
                IWebElement abtCloseButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.CssSelector(".abt-close-icon")));
                abtCloseButton.Click();
            }
            catch (WebDriverTimeoutException) { }
            catch (NoSuchElementException) { }
        }

        private void CloseBannerPopup()
        {
            try
            {
                IWebElement bannerCloseButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.CssSelector(".banner-close-button")));
                bannerCloseButton.Click();
            }
            catch (WebDriverTimeoutException) { }
            catch (NoSuchElementException) { }
        }

        private void RetryAction(Action action, int retries = 3, int delayBetweenRetriesMs = 500)
        {
            for (int i = 0; i < retries; i++)
            {
                try
                {
                    action();
                    break; // Exit if successful
                }
                catch (Exception)
                {
                    if (i == retries - 1)
                        throw; // Rethrow the exception if all retries fail
                    Thread.Sleep(delayBetweenRetriesMs);
                }
            }
        }
         

        [TearDown]
        public void AfterTest()
        {
            var status = TestContext.CurrentContext.Result.Outcome.Status;
            var Stacktrace = TestContext.CurrentContext.Result.StackTrace;
            DateTime time = DateTime.Now;
            String fileName = "screenshot_" + time.ToString("h_mm_ss") + ".png";
            var stacktrace = string.IsNullOrEmpty(TestContext.CurrentContext.Result.StackTrace)
                ? ""
                : string.Format("{0}", TestContext.CurrentContext.Result.StackTrace);
            var errorMessage = TestContext.CurrentContext.Result.Message;

            switch (status)
            {
                case TestStatus.Failed:
                    CaptureScreenshot(driver.Value);
                    break;
                case TestStatus.Skipped:
                     
                    break;
                case TestStatus.Passed:
                    test.Pass("Test Passed");
                    break;
                default:
                    test.Info("Test Finished with no specific status.");
                    break;
            }

            if (driver.Value != null)
            {
                driver.Value.Quit();
                driver.Value.Dispose();
                driver.Value = null;
            }
        }

        public void CaptureScreenshot(IWebDriver driver)
        {
            try
            { 

                var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                var filename = TestContext.CurrentContext.Test.MethodName + "_screenshot_" + DateTime.Now.Ticks + ".png";
                var path = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory) + "/screenshots/"+ filename;
                screenshot.SaveAsFile(path);
                TestContext.AddTestAttachment(path);
                AllureLifecycle.Instance.AddAttachment(filename, "image/png", path);

                 
                 
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error capturing screenshot: " + ex.Message);
              
            }
        }
 
    }
}
