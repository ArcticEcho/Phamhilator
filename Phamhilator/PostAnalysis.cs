using System.Collections.Generic;



namespace Phamhilator
{
    public abstract class PostAnalysis
    {
        public bool AutoTermsFound;
        public float Accuracy;
        public PostType Type;
        public HashSet<FilterConfig> FiltersUsed = new HashSet<FilterConfig>();
        public HashSet<Term> WhiteTermsFound = new HashSet<Term>();
        public HashSet<Term> BlackTermsFound = new HashSet<Term>();
    }
}