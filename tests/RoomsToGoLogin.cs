using OpenQA.Selenium;
using roomstogoseleniumframework.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace roomstogoseleniumframework.tests
{
    public class RoomsToGoLogin : Base
    {
        [Test, Category("Regression")]
        public void TestGetSwiperItems()
        {

            ClassicAssert.AreEqual(driver.Value.Title, "Google");


        }

       
    }
}
