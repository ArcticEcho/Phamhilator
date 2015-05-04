using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phamhilator.Yam.Core
{
    public partial class YamClientLocal
    {
        public enum EventType
        {
            InternalException,
            Question,
            Answer,
            Command,
            Data
        }
    }
}
