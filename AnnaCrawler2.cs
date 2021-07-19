using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace dSelenium
{
    class AnnaCrawler2 : ChromeAbstract
    {
        const string Seed = "https://www.tirumala.org/AnnamacharyaSankeerthanas.aspx";
        const string Bucket = "Pallavi";
        const string SavePoint = "AnnaCrawler2";

        internal AnnaCrawler2() : base(false)
        {
        }

        public void Go()
        {
            var iPage = GetPageCursor() ;
            var jumpTill = (iPage / 5) * 5;
            GoToUrl(Seed);
            var currentPage = GetCurrentPage();
            var totalPages = GetTotalPages();
            if (iPage == totalPages)
            {
                return;
            }

            while (currentPage <= totalPages)
            {
                if (currentPage < jumpTill)
                {
                    // Continue from Last Session
                    currentPage += 5;
                }
                else if (currentPage >= jumpTill && currentPage < iPage)
                {
                    // Continue from Last Session
                    currentPage = iPage;
                }
                else
                {
                    ProcessItems();

                    SavePageCursor(currentPage);
                    currentPage++;
                }
                NavigateToPage(currentPage);
            }
        }

        private void ProcessItems()
        {
            var currentItem = 0;
            var totalItems = GetTotalItems();
            while (currentItem <= totalItems)
            {
                ProcessItem(currentItem);
                currentItem++;
            }
        }

        private void ProcessItem(int currentItem)
        {
            var rows = GetRows();
            if (currentItem >= rows.Count())
            {
                return;
            }
            var row = rows.Skip(currentItem).First();
            var anchorTag = By.TagName("a");
            var inputTag = By.TagName("input");

            var input = row.FindElement(inputTag);
            var val = input.GetAttribute("Value");
            var iVal = int.Parse(val);
            var anchor = row.FindElement(anchorTag);
            var wasProcessed = WasSaved(iVal);
            if (wasProcessed)
            {
                return;
            }
            anchor.Click();
            Save(iVal);
        }



        private int GetTotalItems()
        {
            var rows = GetRows();
            return rows.Count();
        }

        private IEnumerable<IWebElement> GetRows()
        {
            var table = GetElementByClass("gridtb");
            var trTag = By.TagName("tr");
            var anchorTag = By.TagName("a");
            var parentElement = By.XPath("parent::*");
            var rows = table.FindElements(trTag)
                            .Where((row) =>
                            {
                                try
                                {
                                    var anchor = row.FindElement(anchorTag);
                                    var parentTag = anchor.FindElement(parentElement).TagName;
                                    return parentTag == "td";
                                }
                                catch (NoSuchElementException ex)
                                {
                                    return false;
                                }
                            });
            return rows;
        }

        private void SavePageCursor(int currentPage)
        {
            Write(SavePoint, currentPage.ToString());
        }

        private int GetPageCursor()
        {
            var targetFile = new FileInfo(SavePoint);
            if (!targetFile.Exists)
            {
                return 0;
            }
            var sPage = File.ReadAllText(targetFile.FullName);
            var iPage = int.Parse(sPage);
            return iPage;
        }

        private int GetTotalPages()
        {
            var txt = GetElementTextById("cph1_Body_PagesCount");
            var regex = new Regex("Page :(.*?)of(.*?);");
            var match = regex.Match(txt);
            var tot = match.Groups[2].Value;
            return int.Parse(tot);
        }

        private void Save(int index)
        {
            var targetFile = $"{Bucket}\\{index}.txt";
            var content = Source();
            Write(targetFile, content);
        }

        private bool WasSaved(int index)
        {
            var targetFile = $"{Bucket}\\{index}.txt";
            return new FileInfo(targetFile).Exists;
        }

        private void NavigateToPage(int page)
        {
            ClickElementByClass("PagerNormal", (element) =>
                                                {
                                                    var txt = element.Text.Trim();
                                                    return txt == (((page - 1) % 5 == 0) ? ">>" : page.ToString());
                                                });
        }

        private int GetCurrentPage()
        {
            var txt = GetElementTextByClass("PagerCurrent");
            return int.Parse(txt);
        }
    }
}
