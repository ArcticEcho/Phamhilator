using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Ghamhilator
{
    public class PoSTModel
    {
        public ClassificationRating AverageSpamRating
        {
            get
            {
                if (Tags.Length == 0) { return new ClassificationRating(0, 0); }

                var rating = Tags.Sum(x => x.SpamRating.Rating) / Tags.Length;
                var maturity = Tags.Sum(x => x.SpamRating.Maturity) / Tags.Length;

                return new ClassificationRating(rating, maturity);
            }
        }

        public ClassificationRating AverageOffensiveRating
        {
            get
            {
                if (Tags.Length == 0) { return new ClassificationRating(0, 0); }

                var rating = Tags.Sum(x => x.OffensiveRating.Rating) / Tags.Length;
                var maturity = Tags.Sum(x => x.OffensiveRating.Maturity) / Tags.Length;

                return new ClassificationRating(rating, maturity);
            }
        }

        public ClassificationRating AverageLowQualityRating
        {
            get
            {
                if (Tags.Length == 0) { return new ClassificationRating(0, 0); }

                var rating = Tags.Sum(x => x.LowQualityRating.Rating) / Tags.Length;
                var maturity = Tags.Sum(x => x.LowQualityRating.Maturity) / Tags.Length;

                return new ClassificationRating(rating, maturity);
            }
        }

        public PoSTag[] Tags { get; private set; }

        public string ModelID { get; private set; }



        public PoSTModel(PoSTag[] tags, string modelID)
        {
            if (tags == null) { throw new ArgumentNullException("tags"); }
            if (string.IsNullOrEmpty(modelID)) { throw new ArgumentException("'modelID' must not be null or empty.", "modelID"); }

            Tags = tags;
            ModelID = modelID;
        }
    }
}
