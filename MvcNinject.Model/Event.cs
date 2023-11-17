using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcNInject.Model
{
    public class Event
    {
        public int EventID { get; set; }
        public string EventName { get; set; }
        public string EventStarts { get; set; }
        public string EventEnds { get; set; }
        public string EventLocation { get; set; }
    }
}
