using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using System.IO;
using System.Globalization;
using CsvHelper;

namespace Scraper
{
    class Program
    {
        static ScrapingBrowser _browser = new ScrapingBrowser();
        static void Main(string[] args)
        {
            //Console.WriteLine("Enter Search Term:");
            var searchTerm = Console.ReadLine();
            var mainPageLinks = GetMainPageLinks("https://newyork.craigslist.org/d/computer-gigs/search/cpg");
            var lstGigs = GetPageDetails(mainPageLinks, searchTerm);
            exportGigsToCSV(lstGigs, searchTerm);
        }

        static List<string> GetMainPageLinks(string url)
        {
            var mainPageLinks = new List<string>();

            var html = GetHtml(url);

            var links = html.CssSelect("a");

            foreach (var link in links)
            {
                if (link.Attributes["href"].Value.Contains(".html"))
                {
                    mainPageLinks.Add(link.Attributes["href"].Value);
                }
            }

            return mainPageLinks;
        }

        static List<PageDetails> GetPageDetails(List<string> urls, string searchTerm)
        {
            var lstPageDetails = new List<PageDetails>();
            foreach (var url in urls)
            {
                var htmlNode = GetHtml(url);
                var pageDetails = new PageDetails();
                pageDetails.title = htmlNode.OwnerDocument.DocumentNode.SelectNodes("//html/head/title")[0].InnerText;
                
                var description = htmlNode.OwnerDocument.DocumentNode.SelectNodes("//html/body/section/section/section/section")[0].InnerText;
                pageDetails.description = description
                    .Replace("\n        \n            QR Code Link to This Post\n            \n        \n", "");

                pageDetails.url = url;

                if (pageDetails.title.ToLower().Contains(searchTerm.ToLower())
                || pageDetails.description.ToLower().Contains(searchTerm.ToLower()))
                    lstPageDetails.Add(pageDetails);
            }

            return lstPageDetails;
        }

        static void exportGigsToCSV(List<PageDetails> lstPageDetails, string searchTerm)
        {
            using (var writer = new StreamWriter($@"/Users/robertlewis/Desktop/{searchTerm}_{DateTime.Now.ToFileTime()}.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(lstPageDetails);
            }
        }

        static HtmlNode GetHtml(string url)
        {
            WebPage webpage = _browser.NavigateToPage(new Uri(url));
            return webpage.Html;
        }
    }

    public class PageDetails
    {
        public string title { get; set; }
        public string description { get; set; }
        public string url { get; set; }
    }
}
