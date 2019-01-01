using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckBot.Models
{
    public class ProhibitedWordsUserTracker
    {
        public List<string> SentProhibitedWords { get; set; }
        public DateTime SentTime { get; set; }
    }
}
