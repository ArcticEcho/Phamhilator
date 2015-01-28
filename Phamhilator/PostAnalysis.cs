using System.Collections.Generic;



namespace Phamhilator
{
    public abstract class PostAnalysis
    {
        public bool AutoTermsFound;
        public float Accuracy;
        public PostType Type;
        public Dictionary<FilterClass, FilterType> FiltersUsed = new Dictionary<FilterClass, FilterType>();
        public HashSet<Term> WhiteTermsFound = new HashSet<Term>();
        public HashSet<Term> BlackTermsFound = new HashSet<Term>();
    }
}