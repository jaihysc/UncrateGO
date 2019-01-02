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
    }
    public class UserBankingStorage
    {
        public long Credit { get; set; }
        public long CreditDebt { get; set; }
    }
}
