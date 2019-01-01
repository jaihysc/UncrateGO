using DuckBot.Core;
using DuckBot.Models;
using DuckBot.Modules.Finance.CurrencyManager;
using DuckBot.Modules.UserActions;
using DuckBot_ClassLibrary;
using DuckBot_ClassLibrary.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DuckBot.Modules.Finance.ServiceThreads
{
    public class UserBankingInterestUpdater
    {
        ///<Summary>
        ///Updates the user banking debt every set amount of milliseconds, wait duration hardcoded
        ///</Summary>
        public static void UpdateUserDebtInterest()
        {
            while (MainProgram._stopThreads == false)
            {
                try
                {
                    //Log action
                    //Console.WriteLine("User debt updated - " + DateTime.Now);

                    UserDebtInterestUpdater();
                }
                catch (Exception)
                {
                }

                //Sleep for 30 minutes
                Thread.Sleep(1800000);
            }
        }

        ///<Summary>
        ///Increases user borrowed debt by set percentage
        ///</Summary>
        public static void UserDebtInterestUpdater()
        {
            var userStorage = UserDataManager.GetUserStorage();

            //Update user debt
            foreach (var user in userStorage.UserInfo.Values)
            {
                try
                {
                    //Calculate forcefully deduct amount
                    long deductionAmount = 0;
                    //Set deduction to 1 in the event debt is less than 5 and user owns credits
                    if (userStorage.UserInfo[user.UserId].UserBankingStorage.Credit > 0 && userStorage.UserInfo[user.UserId].UserBankingStorage.CreditDebt > 0)
                    {
                        deductionAmount = 1;
                    }

                        deductionAmount = Convert.ToInt64(userStorage.UserInfo[user.UserId].UserBankingStorage.CreditDebt * double.Parse(SettingsManager.RetrieveFromConfigFile("interestRate")));


                    //Calculate new credits
                    long userCreditsNew = 0;
                    //Check if user has sufficient credits
                    if (userStorage.UserInfo[user.UserId].UserBankingStorage.Credit - deductionAmount > 0)
                    {
                        userCreditsNew = userStorage.UserInfo[user.UserId].UserBankingStorage.Credit - deductionAmount;
                    }



                    //
                    //Calculate new debt with interest
                    long debtAmountNew;
                    try
                    {
                        debtAmountNew = Convert.ToInt64((userStorage.UserInfo[user.UserId].UserBankingStorage.CreditDebt * double.Parse(SettingsManager.RetrieveFromConfigFile("interestRate"))) + userStorage.UserInfo[user.UserId].UserBankingStorage.CreditDebt);
                    }
                    catch (OverflowException)
                    {
                        debtAmountNew = long.MaxValue;
                    }


                    //Write to file
                    userStorage.UserInfo[user.UserId].UserBankingStorage.CreditDebt = debtAmountNew;
                    userStorage.UserInfo[user.UserId].UserBankingStorage.Credit = userCreditsNew;

                    UserDataManager.WriteUserStorage(userStorage);

                }
                catch(Exception)
                {
                }
            }
        }
    }
}
