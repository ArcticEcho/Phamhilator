namespace Phamhilator
{
    public struct FilterConfig
    {
        private FilterClass classification;
        private FilterType type;

        public FilterClass Class 
        {
            get 
            { 
                return classification;
            } 
        }

        public FilterType Type
        {
            get 
            { 
                return type;
            } 
        }



        public FilterConfig(FilterClass classification, FilterType type)
        {
            this.classification = classification;
            this.type = type;
        }
    }
}
