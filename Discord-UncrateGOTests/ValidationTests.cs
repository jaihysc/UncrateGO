using Microsoft.VisualStudio.TestTools.UnitTesting;
using UncrateGo.Core;
using UncrateGo.Modules;
using UncrateGo.Modules.Csgo;

namespace Discord_UncrateGoTests
{
    //!! Make sure a paths.txt exists in the execution directory, or else tests requiring stored data will fail
    [TestClass()]
    public class BankingValidationTests
    {
        private readonly ulong _testUserId = TestUser.TestUserId;

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
            if (!BankingHandler.AddCredits(_testUserId, 12))
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void ShouldNotAddCreditsToTestUser()
        {
            if (BankingHandler.AddCredits(_testUserId, -9999999999999))
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void ShouldGetTestUserCredits()
        {
            if (UserDataManager.GetUserCredit(_testUserId) < 0)
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

        [TestMethod()]
        public void ShouldUpdateCsgoCosmeticData()
        {
            CsgoDataHandler.UpdateRootWeaponSkin(null);
        }
    }

    public class TestUser
    {
        public static ulong TestUserId = 502245991953727490;
    }
}