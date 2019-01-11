using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UncrateGo.Models
{
    public class UserStorage
    {
        public Dictionary<ulong, UserInfo> UserInfo { get; set; }
    }
    public class UserInfo
    {
        public ulong UserId { get; set; }
        public UserBankingStorage UserBankingStorage { get; set; }
        public UserCsgoStatsStorage UserCsgoStatsStorage { get; set; }
    }
    public class UserBankingStorage
    {
        public long Credit { get; set; }
    }
    public class UserCsgoStatsStorage
    {
        public long CasesOpened { get; set; }
        public long SouvenirsOpened { get; set; }
        public long DropsOpened { get; set; }
        public long StickersOpened { get; set; }

        public long ConsumerGrade { get; set; }
        public long IndustrialGrade { get; set; }
        public long MilSpecGrade { get; set; }
        public long Restricted { get; set; }
        public long Classified { get; set; }
        public long Covert { get; set; }
        public long Special { get; set; }
        public long Stickers { get; set; }
        public long Other { get; set; }
    }
}
