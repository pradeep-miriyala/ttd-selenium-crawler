using System.Collections.Generic;
using System.IO;
using System.Text;
using HtmlAgilityPack;
using System.Linq;

namespace dSelenium
{
    internal class AnnaParser
    {
        const string Bucket = "Pages";
        const string Seperator = ",";
        public AnnaParser()
        {

        }

        internal void Go()
        {
            var di = new DirectoryInfo(Bucket);
            var rows = new List<string[]>();
            foreach (var fi in di.GetFiles().OrderBy(x => int.Parse(x.Name.Replace(".txt", ""))))
            {
                var fileRows = Process(fi.FullName);
                if (rows.Count == 0)
                {
                    rows.AddRange(fileRows.Take(1));

                }
                rows.AddRange(fileRows.Skip(2));
            }
            Save(rows, "Anna.txt");
        }

        private void Save(List<string[]> rows, string targetFile)
        {
            var sw = new StreamWriter(targetFile, false, Encoding.UTF8);
            foreach (var row in rows)
            {
                for (var i = 0; i < row.Length; i++)
                {
                    var col = row[i];
                    sw.Write(col);
                    if (i == row.Length - 1)
                    {
                        continue;
                    }
                    sw.Write(Seperator);
                }
                sw.WriteLine();
            }
            sw.Close();
        }

        private List<string[]> Process(string fullName)
        {
            var doc = new HtmlDocument();
            doc.Load(fullName);
            var tableNode = doc.DocumentNode.SelectNodes("//table").Where(x => x.HasClass("gridtb")).FirstOrDefault();
            return tableNode
                .Descendants("tr")
                .Where(rowNode => rowNode.SelectNodes("td|th") != null)
                .Select((rowNode) =>
            {
                var columnNodes = rowNode.SelectNodes("td|th");
                var vals = columnNodes.Select((x) =>
                 {
                     var txt = x.InnerText.Trim('\r', '\n', ' ');
                     var hiddenTxt = x.Descendants("input").FirstOrDefault()?.GetAttributeValue("value", "");
                     if (string.IsNullOrEmpty(hiddenTxt))
                     {
                         return txt;
                     }
                     return $"{txt}-{hiddenTxt}";
                 }).ToList();
                var pageId = new FileInfo(fullName).Name.Replace(".txt", "");
                vals.Add(pageId);
                return vals.ToArray();
            }).ToList();
        }
    }
}