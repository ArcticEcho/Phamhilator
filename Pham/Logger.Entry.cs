using System;

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
