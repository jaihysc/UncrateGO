using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UncrateGo.Modules.Interaction
{
    public class UserInteraction
    {
        /// <summary>
        /// Returns a bolded user name of specified user
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string BoldUserName(SocketCommandContext context)
        {
            return $"**{context.Message.Author.ToString().Substring(0, context.Message.Author.ToString().Length - 5)}**";
        }
    }
}
