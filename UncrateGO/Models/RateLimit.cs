using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UncrateGo.Models
{
    public class CommandTimeout
    {
        public uint TimesInvoked { get; set; }
        public DateTime FirstInvoke { get; }
        public bool ReceivedError { get; set; }

        public CommandTimeout(DateTime timeStarted)
        {
            FirstInvoke = timeStarted;
            ReceivedError = false;
        }
    }
}
