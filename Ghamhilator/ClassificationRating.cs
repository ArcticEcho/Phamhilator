using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Ghamhilator
{
    public class ClassificationRating
    {
        private float rat;
        private float mat;

        public float Rating
        {
            get { return rat; }
            set { rat = value; }
        }

        public float Maturity
        {
            get { return mat; }
            set { mat = value; }
        }



        public ClassificationRating(float rating, float maturity)
        {
            rat = rating;
            mat = maturity;
        }
    }
}
