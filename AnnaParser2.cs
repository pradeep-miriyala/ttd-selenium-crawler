using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace dSelenium
{
    internal class AnnaParser2
    {
        const string Bucket = "Pallavi";

        public AnnaParser2()
        {
        }

        internal void Go()
        {
            var di = new DirectoryInfo(Bucket);
            var rows = new List<string[]>();
            foreach (var fi in di.GetFiles().OrderBy(x => int.Parse(x.Name.Replace(".txt", ""))))
            {
                var list = Process(fi);
                rows.Add(list.ToArray());
            }
            Save(rows, "Keertanas.json");
        }

        private void Save(List<string[]> rows, string targetFile)
        {
            using (var sw = new StreamWriter(targetFile))
            using (var writer = new JsonTextWriter(sw))
            {
                new JsonSerializer { Formatting = Newtonsoft.Json.Formatting.Indented }.Serialize(writer, rows);
            }
        }

        private List<string> Process(FileInfo fi)
        {
            var id = fi.Name.Replace(".txt", "");

            var doc = new HtmlDocument();
            doc.Load(fi.FullName);

            var nodeOfInterest = doc.GetElementbyId("popupdiv");
            var spamNodes = nodeOfInterest
                .Descendants()
                .Where(t =>
                {
                    return
                           (t.Name == "div" && t.GetAttributeValue("style", "") == "display:none");
                });
            while (spamNodes.Any())
            {
                var spamNode = spamNodes.First();
                spamNode.RemoveAllChildren();
                spamNode.Remove();
            }

            var brNodes = nodeOfInterest
                .Descendants()
                .Where(t =>
                {
                    return (t.Name == "br");
                });

            while (brNodes.Any())
            {
                var brNode = brNodes.First();
                brNode.ParentNode.InsertAfter(doc.CreateTextNode(Environment.NewLine), brNode);
                brNode.Remove();
            }

            foreach (var element in nodeOfInterest.Descendants().Where(t => t.Name != "#text"))
            {
                element.Attributes.Remove();
            }

            nodeOfInterest.Attributes.Remove();

            var rawXml = CleanXml(nodeOfInterest.OuterHtml);
            var list = ParseItem(rawXml);
            list.Insert(0, rawXml);
            list.Insert(0, id);
            return list;
        }

        private List<string> ParseItem(string rawXml)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(rawXml);
            xDoc.PreserveWhitespace = true;
            var spans = xDoc.GetElementsByTagName("span");
            if (spans.Count > 1)
            {
                Debugger.Break();
            }
            var list = new List<string>();
            foreach (XmlNode child in spans[0])
            {
                list.Add(child.InnerText.Trim('\r', '\n', ' '));
            }
            list.Add("--");
            var trs = xDoc.GetElementsByTagName("tr");
            foreach (XmlNode tr in trs)
            {
                foreach (XmlNode td in tr.ChildNodes)
                {
                    list.Add(td.InnerText.Trim('\r', '\n', ' '));
                }
                list.Add("--");
            }
            return list;
        }

        private string CleanXml(string outerHtml)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(outerHtml);
            var sb = new StringBuilder();
            var settings = new XmlWriterSettings()
            {
                Indent = false,
                OmitXmlDeclaration = true,
                ConformanceLevel = ConformanceLevel.Fragment
            };
            using (var writer = XmlWriter.Create(sb, settings))
            {
                xDoc.WriteTo(writer);
            }
            var cleanXml = sb.ToString();
            return cleanXml;
        }
    }
}