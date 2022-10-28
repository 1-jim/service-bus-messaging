using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingWorkerService.Configuration
{
    public class BusConfiguration
    {
        public string Connection { get; set; }
        public string Queue { get; set; }
    }
}
