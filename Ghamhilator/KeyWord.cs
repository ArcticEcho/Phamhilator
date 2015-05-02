using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Ghamhilator
{
    public class KeyWord
    {
        private string word;
        private float score;

        public string Word { get { return word; } }
        public ClassificationRating SpamRating { get; set; }
        public ClassificationRating OffensiveRating { get; set; }
        public ClassificationRating LowQualityRating { get; set; }



        public KeyWord(string word, ClassificationRating spamRating, ClassificationRating offensiveRating, ClassificationRating lowQualityRating)
        {
            this.word = word;
            SpamRating = spamRating;
            OffensiveRating = offensiveRating;
            LowQualityRating = lowQualityRating;
        }
    }
}
