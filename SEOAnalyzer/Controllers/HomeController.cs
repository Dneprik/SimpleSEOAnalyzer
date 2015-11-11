using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using HtmlAgilityPack;
using SEOAnalyzer.Models;

namespace SEOAnalyzer.Controllers
{
    public class HomeController : Controller
    {
        //Chars/Strings which will excluded from searching
        private readonly char[] ignorChars =
        {
            ' ', '.', ',', ':', '\\', '!', '#', '$', '%', '–', '-', '+', '*', '/', '1',
            '2', '3', '4', '5', '6', '7', '8', '9', '0'
        };

        private string[] ignoreStrings = {"", " ", " a ", " an ", " and ", " if ", " or ", " the ", " for "};
        //Collection with all keyword in meta tags and all words on page
        private List<string> metaTags = new List<string>();
        private List<string> pageWords = new List<string>();
        //Colletion with result of count keywords and words on page
        private List<Word> repWordsFromMeta = new List<Word>();
        private List<Word> repWordsOnPage = new List<Word>();
        //Uri of target website
        private Uri uri { get; set; }

        public ActionResult Index()
        {
            return View();
        }

        //Main method of Analyzing
        public JsonResult Analyze(string url)
        {
            metaTags.Clear();
            pageWords.Clear();
            repWordsFromMeta.Clear();
            repWordsOnPage.Clear();
            //Add http to url if it wasn't.
            url = UrlVerification(url);
            try
            {
                uri = new Uri(url);
                //Get HTML page
                var content = DownloadContent(uri);
                //Count external links
                var countLinks = GetCountOfExternalLinks(content, uri);
                //Get keywords and all words from page
                metaTags = GetMetaTags(content);
                pageWords = GetAllWords(content);

                //Get a count occurrences each word on the page
                repWordsOnPage = GetCountsWords(pageWords, pageWords);

                //Number of occurrences on the page of each word listed in meta tags(keywords)
                repWordsFromMeta = GetCountsWords(pageWords, metaTags);

                var result = new RootJsonObject
                {
                    Result = true,
                    RepWordsFromMeta = repWordsFromMeta,
                    RepWordsOnPage = repWordsOnPage,
                    CountLinks = countLinks
                };

                return Json(result);
            }

            catch (Exception ex)
            {
                var rj = new RootJsonObject {Result = false, Message = ex.Message};
                return Json(rj);
            }
        }

        private string UrlVerification(string url)
        {
            var pattern = @"(^http)|(^https)";
            if (!Regex.IsMatch(url, pattern, RegexOptions.IgnoreCase))
            {
                url = "http://" + url;
            }

            return url;
        }


        private HtmlDocument DownloadContent(Uri url)
        {
            var hw = new HtmlWeb();
            var content = hw.Load(url.AbsoluteUri);
            return content;
        }

        private int GetCountOfExternalLinks(HtmlDocument content, Uri url)
        {
            var countLinks = 0;
            var pattern = @"(^javascript)|(^#)|(" + url.Host + ")";
            foreach (var link in content.DocumentNode.SelectNodes("//a[@href]"))
            {
                if (!Regex.IsMatch(link.Attributes["href"].Value, pattern, RegexOptions.IgnoreCase))

                    countLinks++;
            }
            return countLinks;
        }


        private List<string> GetMetaTags(HtmlDocument content)
        {
            var keywords = new List<string>();
            try
            {
                foreach (var link in content.DocumentNode.SelectNodes("//meta[@name='keywords']"))
                {
                    keywords.AddRange(
                        link.Attributes["content"].Value.Split(',').Select(s => s.Trim().ToLower()).ToList());
                }

                return keywords;
            }
            catch (Exception ex)
            {
                return keywords;
            }
        }

        private List<string> GetAllWords(HtmlDocument content)
        {
            var words = new List<string>();
            var nodes = content.DocumentNode.Descendants().Where(n =>
                n.NodeType == HtmlNodeType.Text &&
                n.ParentNode.Name != "script" &&
                n.ParentNode.Name != "style");
            foreach (var node in nodes)
            {
                if (!string.IsNullOrEmpty(node.InnerText.Trim()))
                    words.AddRange(node.InnerText.Trim().Split(ignorChars).Select(s => s.Trim().ToLower()));
            }

            words = words.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            return words;
        }

        private List<Word> GetCountsWords(List<string> words, List<string> comparedWords)
        {
            var countWords = new List<Word>();

            if (words != null)
            {
                foreach (var item in comparedWords.Distinct())
                {
                    countWords.Add(new Word {Name = item, Count = words.Where(x => x == item).Count().ToString()});
                }
            }

            return countWords;
        }
    }
}