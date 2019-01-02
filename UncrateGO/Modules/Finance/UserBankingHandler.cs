using Discord.Commands;
using Discord.WebSocket;
using UncrateGo.Modules.UserActions;
using System.Globalization;

namespace UncrateGo.Modules.Finance.CurrencyManager
{
    public class UserBankingHandler
    {
        public static void CheckIfUserCreditProfileExists(SocketGuildUser user)
        {
            var userStorage = UserDataManager.GetUserStorage();
            //Create txt user credit entry if user does not exist
            if (!userStorage.UserInfo.TryGetValue(user.Id, out var i))
            {
                //Create user profile
                UserDataManager.CreateNewUserXmlEntry(user);
            }
        }

        /// <summary>
        /// Formats the currency with spaces, as well as a decimal place
        /// </summary>
        /// <param name="inputCredits"></param>
        /// <returns></returns>
        public static string CreditCurrencyFormatter(long inputCredits)
        {
            //Formats number to use currency numeration
            var numberGroupSeperator = new NumberFormatInfo { NumberGroupSeparator = " " };

            inputCredits = inputCredits / 100;

            return inputCredits.ToString("N0", numberGroupSeperator);
        }
    }
}
