using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phamhilator.Pham.UI
{
    public class PostCheckBackEntry
    {
        public string Url { get; set; }

        public string Body { get; set; }

        public DateTime NextScheduledCheck { get; set; }

        public ushort PreviousChecks { get; set; }
    }
}
