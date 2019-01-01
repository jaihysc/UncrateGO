using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckBot.Models
{
    public class GuildRoleStorage
    {
        public List<GuildRoleEntry> GuildRoles { get; set; }
    }
    public class GuildRoleEntry
    {
        public ulong GuildID { get; set; }
        public string RoleName { get; set; }
        public ulong GuildRoleID { get; set; }
    }
}
