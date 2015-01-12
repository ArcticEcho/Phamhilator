using System;
using System.Collections.Generic;



namespace Phamhilator
{
    public class LogItem
    {
        public string Url { get; set; }
        public string Site { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public DateTime TimeStamp { get; set; }
        public PostType ReportType { get; set; }
        public List<JsonTerm> BlackTerms { get; set; }
        public List<JsonTerm> WhiteTerms { get; set; }
    }
}
