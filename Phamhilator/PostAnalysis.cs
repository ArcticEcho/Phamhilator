using System.Collections.Generic;



namespace Phamhilator
{
    public abstract class PostAnalysis
    {
        public bool AutoTermsFound;
        public PostType Type;
        public float Accuracy;
        public readonly List<FilterType> FiltersUsed = new List<FilterType>();
        public readonly HashSet<Term> WhiteTermsFound = new HashSet<Term>();
        public readonly HashSet<Term> BlackTermsFound = new HashSet<Term>();
    }
}