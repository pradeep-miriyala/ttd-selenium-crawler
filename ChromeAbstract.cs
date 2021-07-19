using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace dSelenium
{
    abstract class ChromeAbstract : IDisposable
    {
        internal ChromeAbstract(bool headLess)
        {
            Driver = GetChromeDriver(headLess);
        }

        protected RemoteWebDriver GetChromeDriver(bool headLess)
        {
            var options = new ChromeOptions();
            if (headLess)
            {
                options.AddArguments(new List<string>() { "headless" });
            }
            return new ChromeDriver(ChromeDriverService.CreateDefaultService(), options);
        }

        protected void GoToUrl(string url)
        {
            Driver.Navigate().GoToUrl(url);
        }

        protected string GetElementTextById(string id)
        {
            var item = Driver.FindElementById(id);
            if (item == null)
            {
                Console.WriteLine($"Element with ID: {id} - not found");
                return null;
            }
            return item.Text;
        }
        protected IWebElement GetElementByClass(string className)
        {
            return Driver.FindElementByClassName(className);
        }

        protected string GetElementTextByClass(string className)
        {
            var item = Driver.FindElementsByClassName(className).FirstOrDefault();
            if (item == null)
            {
                Console.WriteLine($"Element with Class: {className} - not found");
                return null;
            }
            return item.Text;
        }

        protected void ClickElementByClass(string className, Func<IWebElement, bool> whereFunc)
        {
            var item = Driver.FindElementsByClassName(className).Where(whereFunc).FirstOrDefault();
            if (item == null)
            {
                Console.WriteLine($"Element with Class: {className} - not found");
                return;
            }
            item.Click();
        }

        protected void Write(string targetFile, string content)
        {
            var fileInfo = new FileInfo(targetFile);
            if (!fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }
            File.WriteAllText(fileInfo.FullName, content);
        }


        protected string Source() { return Driver.PageSource; }

        public void Dispose()
        {
            Driver.Quit();
        }

        ~ChromeAbstract()
        {
            Dispose();
        }

        protected readonly RemoteWebDriver Driver;

    }
}
