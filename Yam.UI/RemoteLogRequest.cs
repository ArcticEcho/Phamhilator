using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phamhilator.Yam.UI
{
    public class RemoteLogRequest
    {
        public string SearchBy { get; set; }
        public string Key { get; set; }
        public bool ExactMatch { get; set; }
        public bool? FetchQuestions { get; set; }
    }
}
