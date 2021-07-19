using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

namespace dSelenium
{
    class AnnaCrawler : ChromeAbstract
    {
        const string Seed = "https://www.tirumala.org/AnnamacharyaSankeerthanas.aspx";
        const string Bucket = "Pages";
        const string SavePoint = "AnnaCrawler2";
        internal AnnaCrawler() : base(false)
        {
        }

        public void Go()
        {
            var lastPage = GetLatPageSaved();
            GoToUrl(Seed);
            var currentPage = GetCurrentPage();
            var totalPages = GetTotalPages();
            if (lastPage == totalPages)
            {
                return;
            }

            while (currentPage <= totalPages)
            {
                if (currentPage < lastPage)
                {
                    // Continue from Last Session
                    currentPage += 5;
                }
                else
                {
                    Save(currentPage);
                    currentPage++;
                }
                NavigateToPage(currentPage);
            }
        }

        private int GetLatPageSaved()
        {
            var targetFile = new DirectoryInfo(Bucket).GetFiles("*.txt").OrderByDescending(x => x.LastWriteTime).FirstOrDefault();
            if (targetFile == null)
            {
                return 0;
            }
            var sPage = targetFile.Name.Replace(".txt", "");
            var iPage = int.Parse(sPage);
            iPage = (iPage / 5) * 5;
            return iPage < 6 ? 0 : iPage - 5;
        }

        private int GetTotalPages()
        {
            var txt = GetElementTextById("cph1_Body_PagesCount");
            var regex = new Regex("Page :(.*?)of(.*?);");
            var match = regex.Match(txt);
            var tot = match.Groups[2].Value;
            return int.Parse(tot);
        }

        private void Save(int currentPage)
        {
            var targetFile = $"{Bucket}\\{currentPage}.txt";
            var content = Source();
            Write(targetFile, content);
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
