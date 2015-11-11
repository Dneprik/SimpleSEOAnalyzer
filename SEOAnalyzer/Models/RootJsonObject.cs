using System.Collections.Generic;

namespace SEOAnalyzer.Models
{
    public class RootJsonObject
    {
        public List<Word> RepWordsFromMeta { get; set; }
        public List<Word> RepWordsOnPage { get; set; }
        public int CountLinks { get; set; }
        public bool Result { get; set; }
        public string Message { get; set; }
    }
}