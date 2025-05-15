using OpenQA.Selenium;
using roomstogoseleniumframework.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using NUnit.Allure.Core;

namespace roomstogoseleniumframework.tests
{
    [AllureNUnit]
    public class RoomsToGoLogin : Base
    {
        [Test, Category("Regression")]
        public void TestGetSwiperItems()
        {
            Console.WriteLine(driver.Value.Title);
            ClassicAssert.AreEqual(driver.Value.Title, "Google");
            

        }

       
    }
}
