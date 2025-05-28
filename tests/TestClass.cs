using OpenQA.Selenium;
using roomstogoseleniumframework.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using NUnit.Allure.Core;
using Allure.Commons;
using NUnit.Framework.Interfaces;
using Microsoft.VisualStudio.TestPlatform.CrossPlatEngine;
using NUnit.Allure.Attributes;
namespace roomstogoseleniumframework.tests
{
    [AllureNUnit]
    public class TestClass :Base 
    {
         [Test, Category("Regression"), Category("Regression2")]
        public void TestGetSwiperItems()
         {
            test();


        }

        [AllureStep]
        public void test() {
            Console.WriteLine(driver.Value.Title);
            Assert.That(driver.Value.Title, Is.EqualTo("Google"));
            CaptureScreenshot(driver.Value);
        }

        






 



    }
}
