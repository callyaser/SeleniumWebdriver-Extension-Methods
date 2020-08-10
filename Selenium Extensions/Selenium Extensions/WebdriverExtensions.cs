using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Selenium_Extensions
{
    public static class WebdriverExtensions
    {
        public class ElementNotFoundException : Exception
        {
            public ElementNotFoundException(By by)
            {
                throw new Exception($"**UI Element with locator - '{by}' doesn't exist on page!!**");
            }
        }
        public class IncorrectLandingPageException : Exception
        {
            public IncorrectLandingPageException(string str)
            {
                throw new Exception($"**{str}**!!");
            }
        }
        public static IWebElement FindElementWithScroll(this IWebDriver driver, By by)
        {
            var elementExists = driver.IsElementPresent(by);
            if (elementExists)
            {
                IWebElement elt = driver.FindElement(by);
                IJavaScriptExecutor je = (IJavaScriptExecutor)driver;
                je.ExecuteScript("return (document.readyState == 'complete')");
                //je.ExecuteScript("arguments[0].scrollIntoView(true);", elt);
                return elt;
            }
            else
                throw new ElementNotFoundException(by);
        }
        public static IWebElement FindTextboxElementWithScroll(this IWebDriver driver, By by)
        {
            var elementExists = driver.IsElementPresent(by);
            if (elementExists)
            {
                IWebElement elt = driver.FindElement(by);
                IJavaScriptExecutor je = (IJavaScriptExecutor)driver;
                //je.ExecuteScript("return (document.readyState == 'complete')");
                je.ExecuteScript("arguments[0].scrollIntoView(true);", elt);
                je.ExecuteScript("arguments[0].style.outline = '2px solid #3DEF96'", elt);
                return elt;
            }
            else
                throw new ElementNotFoundException(by);
        }
        public static IWebElement FindElementAndHighlight(this IWebDriver driver, By by)
        {
            var elementExists = driver.IsElementPresent(by);
            if (elementExists)
            {
                var elt = driver.FindElement(by);
                var jsDriver = (IJavaScriptExecutor)driver;
                //jsDriver.ExecuteScript("arguments[0].scrollIntoView(true);", elt); // Don't uncomment.
                jsDriver.ExecuteScript("arguments[0].style.outline = '2px solid #3DEF96'", elt);
                return elt;
            }
            else
                throw new ElementNotFoundException(by);
        }
        public static IEnumerable<IWebElement> FindElements_And_Highlight(this IWebDriver driver, By by)
        {
            var elementExists = driver.IsElementPresent(by);
            if (elementExists)
            {
                var elts = driver.FindElements(by);
                var jsDriver = (IJavaScriptExecutor)driver;
                foreach (var elt in elts)
                {
                    jsDriver.ExecuteScript("arguments[0].style.outline = '2px solid #3DEF96'", elt);
                }
                return elts;
            }
            else
                throw new ElementNotFoundException(by);
        }
        public static void ClickElementJS(this IWebDriver driver, By by)
        {
            Thread.Sleep(500);
            var elt = driver.FindElement(by);
            var jsDriver = (IJavaScriptExecutor)driver;
            jsDriver.ExecuteScript("arguments[0].click();", elt);
            driver.WaitforPageLoad();
        }
        public static void PressTab(this IWebDriver driver)
        {
            Actions act = new Actions(driver);
            act.SendKeys(Keys.Tab).Build().Perform();
        }
        public static void ClickElement(this IWebDriver driver, By by)
        {
            Thread.Sleep(150);
            driver.FindElement(by).Click();
            if (!driver.IsAlertPresent())
                driver.WaitforPageLoad();
        }
        public static bool IsAlertPresent(this IWebDriver driver)
        {
            try
            {
                driver.SwitchTo().Alert();
                return true;
            }
            catch (NoAlertPresentException)
            {
                return false;
            }
        }
        public static void WaitforPageLoad(this IWebDriver driver)
        {
            IJavaScriptExecutor je = (IJavaScriptExecutor)driver;
            je.ExecuteScript("return (document.readyState == 'complete')");
        }
        public static void ClickElementAfterWait(this IWebDriver driver, By by)
        {
            Thread.Sleep(200);
            driver.FindElementAndHighlight(by).Click();
            driver.WaitforPageLoad();
        }
        public static void SelectRandomValueFromDropdown(this IWebElement element)
        {
            int i = 0;
            var elt = new SelectElement(element);
            var maxIndexOfDrodown = elt.Options.Count;
            var random = new Random();
            elt.SelectByIndex(random.Next(1, maxIndexOfDrodown));
            var selectedText = elt.SelectedOption.Text;
            while (selectedText.IsNullOrEmpty())
            {
                i++;
                elt.SelectByIndex(random.Next(1, maxIndexOfDrodown));
                selectedText = elt.SelectedOption.Text;
                if (i > 5)
                    throw new Exception($"**Nothing to select. Dropdown Empty!!!!");
            }
        }
        public static void SelectValueFromDropdown(this IWebElement element, string valueToSelect)
        {
            var selectElement = new SelectElement(element);
            if (selectElement.GetAllValuesFromDropdown().Any(s => s == valueToSelect))
                selectElement.SelectByValue(valueToSelect);
            else
                throw new Exception($"The value intended to select-'{valueToSelect}' is not contained in the dropdown");
        }
        public static void SelectFromDropdown(this IWebElement element, string optionToSelect)
        {
            var selectElement = new SelectElement(element);
            if (selectElement.GetAllValuesFromDropdown().Any(s => s.Contains(optionToSelect)))
                selectElement.SelectByValue(optionToSelect);
            else
                throw new Exception($"There's no option in the dropdown that matches-'{optionToSelect}'!!!***");
        }
        public static void SelectTextFromDropdown(this IWebElement element, string textToSelect)
        {
            var elt = new SelectElement(element);
            if (elt.GetAllTextFromDropdown().Any(s => s == textToSelect))
                elt.SelectByText(textToSelect);
            else
                throw new Exception($"The text intended to select-'{textToSelect}' is not contained in the dropdown");
        }
        public static void SelectPaymentMethod(this IWebElement element, string paymentoption)
        {
            string optionToChoose = null;
            var elt = new SelectElement(element);
            foreach (var el in elt.Options)
            {
                if (el.Text.ToLower().Contains(paymentoption))
                {
                    optionToChoose = el.Text;
                    elt.SelectByText(optionToChoose);
                    return;
                }
            }
            throw new Exception($"Payment method- {paymentoption} is not present in the dropdown");
        }
        public static bool IsValuePresentInDropdown(this IWebDriver driver, By by, string valueToChooseFromDropdown)
        {
            if (driver.IsElementPresent(by))
            {
                var elt = driver.FindElement(by);
                var selectElement = new SelectElement(elt);
                return selectElement.GetAllValuesFromDropdown().Any(s => s == valueToChooseFromDropdown);
            }
            else
                throw new Exception($"Element {by} not present!!!");
        }
        public static string SelectedText(this IWebElement elt)
        {
            return new SelectElement(elt).SelectedOption.Text;
        }
        //Use it only if the webelement is a checkbox
        public static bool IsChecked(this IWebElement elt)
        {
            return elt.Selected;
        }
        //usage - driver.FindElement(merchantDropdown"2");
        public static IEnumerable<string> GetAllValuesFromDropdown(this SelectElement elt)
        {
            List<string> Values = new List<string>();
            foreach (var option in elt.Options)
            {
                Values.Add("get_the_value_for_the_element");
            }
            return Values;
        }
        static IEnumerable<string> GetAllTextFromDropdown(this SelectElement elt)
        {
            List<string> Texts = new List<string>();
            foreach (var option in elt.Options)
            {
                Texts.Add(option.Text);
            }
            return Texts;
        }
        public static long GetChildrenCount(this IWebElement element, IWebDriver driver)
        {
            var jsDriver = (IJavaScriptExecutor)driver;
            return (long)jsDriver.ExecuteScript("return arguments[0].children.length", element);
        }
        public static void EnterDate(this IWebDriver driver, By by, string date)
        {
            var elt = driver.FindElement(by);
            var jsDriver = (IJavaScriptExecutor)driver;
            jsDriver.ExecuteScript($"arguments[0].value = '{date}'", elt);
        }
        public static void EnterDate(this IWebDriver driver, string IDOfTheDatepicker, string date)
        {
            driver.FindElementAndHighlight(By.Id(IDOfTheDatepicker));
            var js = driver as IJavaScriptExecutor;
            js.ExecuteScript($"document.getElementById('{IDOfTheDatepicker}').value = '{date}'");
        }
        public static void CaptureScreenshot(this IWebDriver driver, string textToAppend = "")
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            // wait.Until(ExpectedConditions.ElementToBeClickable(By.LinkText("Paysafe")));
            var path = "fake_path";
            var img = (driver as ITakesScreenshot).GetScreenshot();
            var fromPoint = driver.Url.LastIndexOf("/") + 1;
            string filename = driver.Url.Substring(fromPoint).RemoveInvalidCharacters() + (!textToAppend.IsNullOrEmpty() ? $" ({textToAppend})" : "");
            if (File.Exists($@"{path}\{filename}.png"))
            {
                var filenameTemp = filename;
                int i = 2;
                if (textToAppend.IsNullOrEmpty())
                {
                    while (File.Exists($@"{path}\{filenameTemp}.png"))
                    {
                        filenameTemp = filename + i.ToString();
                        i++;
                    }
                }
                else
                {
                    var temp = filenameTemp.Substring(0, filenameTemp.IndexOf("(")).Trim();
                    while (File.Exists($@"{path}\{filenameTemp}.png"))
                    {
                        filenameTemp = temp + i.ToString() + $" ({textToAppend})";
                        i++;
                    }
                }
                filename = filenameTemp;
            }
            img.SaveAsFile($@"{path}\{filename}.png", ScreenshotImageFormat.Png);
            Console.WriteLine($" Screenshot Created - {filename}");
        }
        public static void OpenInNewTab(this IWebDriver driver, string url)
        {
            // Thread.Sleep(2500);
            var js = driver as IJavaScriptExecutor;
            js.ExecuteScript("window.open();");
            driver.SwitchToLatestTab();
            driver.Url = url;
        }
        public static void WaitForAjaxToLoad(this IWebDriver driver, bool RaiseTimeoutException = true)
        {
            if (!driver.Url.Contains("welcome"))
                return;
            var type = new StackTrace().GetFrame(1).GetMethod().ReflectedType.Name;
            DateTime initial = DateTime.Now;
            while (DateTime.Now.Subtract(initial).TotalSeconds < 120)
            {
                var ajaxIsComplete = (bool)(driver as IJavaScriptExecutor).ExecuteScript("return jQuery.active == 0");
                if (ajaxIsComplete)
                {
                    var timeElapsed = DateTime.Now.Subtract(initial);
                    if (type.Contains("LoginPage") | driver.Url.Contains("admin/welcome.asp"))
                        Console.WriteLine($"Time taken to load all widgets - {Math.Round(timeElapsed.TotalSeconds, 3)} secs");
                    else
                        Console.WriteLine($"Time taken to load the widget- {Math.Round(timeElapsed.TotalSeconds, 3)} secs");
                    return;
                }
            }
            if (RaiseTimeoutException)
            {
                throw new Exception("Widgets loading for more than a minute. WebDriver timed out.!!");
            }
        }
        public static void SwitchToLatestTab(this IWebDriver driver)
        {
            var handlesCount = driver.WindowHandles.Count;
            if (handlesCount > 0)
                driver.SwitchTo().Window(driver.WindowHandles[handlesCount - 1]);
        }
        public static void CloseLatestTabAndSwitchToParentTab(this IWebDriver driver)
        {
            driver.Close();
            driver.SwitchTo().Window(driver.WindowHandles.Last());
        }
        public static bool IsElementPresent(this IWebDriver driver, By by)
        {
            driver.ImplicitWait(TimeSpan.FromMilliseconds(4000));
            Thread.Sleep(500);
            return driver.FindElements(by).Count > 0;
        }
        public static bool IsElementPresent(this IWebDriver driver, By by, TimeSpan timeoutInMilliSeconds)
        {
            driver.ImplicitWait(timeoutInMilliSeconds);
            return driver.FindElements(by).Count > 0;
        }
        public static string AcceptAlert(this IWebDriver driver)
        {
            Thread.Sleep(1000);
            var alert = driver.SwitchTo().Alert();
            var alertText = alert.Text;
            alert.Accept();
            return alertText;
        }
        public static void ImplicitWait(this IWebDriver driver, TimeSpan t)
        {
            driver.Manage().Timeouts().ImplicitWait = t;
            //Webpage.tspan = driver.Manage().Timeouts().ImplicitWait;
        }
    }
}
