using System.Collections.Generic;

namespace UncrateGo.Models
{
    public class HelpMenuCommands
    {
        public List<HelpMenuCommandEntry> CommandHelpEntry { get; set; }
    }
    public class HelpMenuCommandEntry
    {
        public string CommandName { get; set; }
        public string CommandDescription { get; set; }
        public string CommandRequiredPermissions { get; set; }
        public string CommandUsage { get; set; }
        public string CommandUsageDefinition { get; set; }
    }
}
