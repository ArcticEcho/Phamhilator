using System.Collections.Generic;
using Phamhilator.Yam.Core;
using ServiceStack.Text;

namespace Phamhilator.Yam.UI
{
    internal class LogReader
    {
        public IEnumerable<LogEntry> GetEnteries()
        {
            var data = DataManager.LoadLines("Yam", "Post Log");

            foreach (var line in data)
            {
                yield return JsonSerializer.DeserializeFromString<LogEntry>(line);
            }
        }
    }
}
