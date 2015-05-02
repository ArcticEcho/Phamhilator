using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Ghamhilator
{
    public class PoSTag
    {
        public string Tag { get; private set; }
        public ClassificationRating SpamRating { get; set; }
        public ClassificationRating OffensiveRating { get; set; }
        public ClassificationRating LowQualityRating { get; set; }
        public HashSet<KeyWord> BlackKeyWords { get; private set; }
        public HashSet<KeyWord> WhiteKeyWords { get; private set; }



        public PoSTag(string tag, ClassificationRating spamRating = null, ClassificationRating offensiveRating = null, ClassificationRating lowQualityRating = null, HashSet<KeyWord> blackWords = null, HashSet<KeyWord> whiteWords = null)
        {
            if (String.IsNullOrEmpty(tag)) { throw new ArgumentException("'tag' must not be null or empty.", "tag"); }

            Tag = tag;
            SpamRating = spamRating ?? new ClassificationRating(1, 1);
            OffensiveRating = offensiveRating ?? new ClassificationRating(1, 1);
            LowQualityRating = lowQualityRating ?? new ClassificationRating(1, 1);
            BlackKeyWords = blackWords ?? new HashSet<KeyWord>();
            WhiteKeyWords = whiteWords ?? new HashSet<KeyWord>();
        }
    }
}
