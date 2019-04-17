using Microsoft.VisualStudio.TestTools.UnitTesting;
using UncrateGo;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UncrateGo.Core;
using UncrateGo.Modules;
using UncrateGo.Modules.Csgo;

namespace UncrateGo.Tests
{
    //!! Make sure a paths.txt exists in the execution directory, or else tests requiring stored data will fail
    [TestClass()]
    public class BankingValidationTests //TODO, write some automated tests to make my life easier
    {
        private ulong TestUserId = TestUser.TestUserId;

        [TestMethod()]
        public void ShouldFormatCurrencyWithSpaces()
        {
            string result = BankingHandler.CurrencyFormatter(123456789);
            if (result != "123 456 789")
            {
                Assert.Fail();
            }

            result = BankingHandler.CurrencyFormatter(-987654321);
            if (result != "-987 654 321")
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void ShouldNotFindUserAddCredits()
        {
            if (BankingHandler.AddCredits(123456789, 12))
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void ShouldAddCreditsToTestUser()
        {
            if (!BankingHandler.AddCredits(TestUserId, 12))
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void ShouldNotAddCreditsToTestUser()
        {
            if (BankingHandler.AddCredits(TestUserId, -9999999999999))
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void ShouldGetTestUserCredits()
        {
            if (UserDataManager.GetUserCredit(TestUserId) < 0)
            {
                Assert.Fail();
            }
        }


    }

    [TestClass()]
    public class DiscordBotListValidationTests
    {
        [TestMethod()]
        public void ShouldUpdateDiscordBotsListStats()
        {
            DiscordBotsListUpdater.UpdateDiscordBotsListInfo(null);
        }
    }

    [TestClass()]
    public class CsgoValidationTests
    {
        [TestMethod()]
        public void ShouldGetCsgoCosmeticData()
        {
            if (CsgoDataHandler.GetCsgoCosmeticData() == null)
            {
                Assert.Fail();
            }
        }

        //[TestMethod()]
        //public void ShouldUpdateCsgoCosmeticData()
        //{
        //    CsgoDataHandler.UpdateRootWeaponSkin(null);
        //}
    }

    public class TestUser
    {
        public static ulong TestUserId = 502245991953727490;
    }
}