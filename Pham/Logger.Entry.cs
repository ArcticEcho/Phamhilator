using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phamhilator.Pham.UI
{
    public partial class Logger<T>
    {
        public class Entry
        {
            public object Data { get; set; }

            public DateTime Timestamp { get; set; }
        }
    }
}
