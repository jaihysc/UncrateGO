using System.Collections.Generic;
using System.Linq;
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
    public class CsgoDataValidationTests
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
            CsgoDataUpdater.UpdateRootWeaponSkin();
        }

        [TestMethod()]
        public void ShouldGetCsgoCase()
        {
            var result = CsgoDataHandler.GetCsgoCase("Danger Zone Case");
            if (result == null)
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void ShouldNotGetCsgoCase()
        {
            var result = CsgoDataHandler.GetCsgoCase("~~~eqasdase1e1dascqd | This case does not exist sadsa");
            if (result != null)
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void ShouldNotGetUserItems()
        {
            List<UserSkinEntry> result = CsgoDataHandler.GetUserItems(000000000);
            if (result.Any())
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void ShouldGetUserItems()
        {
            List<UserSkinEntry> result = CsgoDataHandler.GetUserItems(TestUser.TestUserId);
            if (!result.Any()) //Sometimes the user may not actually have any items
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void ShouldFuzzyFindSkinDataItem()
        {
            var cosmeticData = CsgoDataHandler.GetCsgoCosmeticData();

            //Get the weapon skin specified
            SkinDataItem marketSkin = CsgoTransactionHandler.FuzzyFindSkinDataItem(cosmeticData.ItemsList.Values.ToList(), "glock").FirstOrDefault();
            if (marketSkin == null)
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void ShouldNotFuzzyFindSkinDataItem()
        {
            var cosmeticData = CsgoDataHandler.GetCsgoCosmeticData();

            //Get the weapon skin specified
            SkinDataItem marketSkin = CsgoTransactionHandler.FuzzyFindSkinDataItem(cosmeticData.ItemsList.Values.ToList(), "1ecfwefbhewr5132").FirstOrDefault();
            if (marketSkin != null)
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void ShouldFuzzyFindUserSkinEntries()
        {
            List<UserSkinEntry> userItems = CsgoDataHandler.GetUserItems(TestUser.TestUserId);
            var result = CsgoTransactionHandler.FuzzyFindUserSkinEntries(userItems, "Acid Fade").FirstOrDefault();

            if (result == null)
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void ShouldNotFuzzyFindUserSkinEntries()
        {
            List<UserSkinEntry> userItems = CsgoDataHandler.GetUserItems(TestUser.TestUserId);
            var result = CsgoTransactionHandler.FuzzyFindUserSkinEntries(userItems, "1ecfwefbhewr5132").FirstOrDefault();

            if (result != null)
            {
                Assert.Fail();
            }
        }
    }
    
    [TestClass()]
    public class CoreValidationTests
    {
        [TestMethod()]
        public void ShouldFindSimilarItemsByWords()
        {
            var testPhrases = new List<string>
            {
                "He told us a very exciting adventure story.",
                "Don't step on the broken glass.",
                "The memory we used to share is no longer coherent.",
                "If Purple People Eaters are real… where do they find purple people to eat?",
                "Two seats were vacant.",
                "The lake is a long way from here.",
                "If I don’t like something, I’ll stay away from it.",
                "He said he was not there yesterday; however, many people saw him there.",
                "He ran out of money, so he had to stop playing poker.",
                "She folded her handkerchief neatly.",
                "How was the math test?",
                "I am happy to take your donation; any amount will be greatly appreciated.",
                "What was the person thinking when they discovered cow’s milk was fine for human consumption… and why did they do it in the first place!?",
                "Should we start class now, or should we wait for everyone to get here?",
                "This is a Japanese doll.",
            };

            if (FuzzySearch.FindSimilarItemsByWords(testPhrases, "He told us a very exciting adventure story")
                    .FirstOrDefault() != "He told us a very exciting adventure story.") Assert.Fail();

            if (FuzzySearch.FindSimilarItemsByWords(testPhrases, "I happy to your")
                    .FirstOrDefault() != "I am happy to take your donation; any amount will be greatly appreciated.") Assert.Fail();

            if (FuzzySearch.FindSimilarItemsByWords(testPhrases, "ike something, I’ll s")
                    .FirstOrDefault() != "If I don’t like something, I’ll stay away from it.") Assert.Fail();

            if (FuzzySearch.FindSimilarItemsByWords(testPhrases, "THIS A JAPANESE doLL")
                    .FirstOrDefault() != "This is a Japanese doll.") Assert.Fail();
        }

        [TestMethod()]
        public void ShouldComputeFuzzyDistance()
        {
            if (FuzzySearch.Compute("1234567890abcdefgh", "1234565890abcdefgh") > 2) Assert.Fail();
        }
    }

    public class TestUser
    {
        public static ulong TestUserId = 502245991953727490;
    }
}