using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
            DiscordBotsListUpdater.UpdateDiscordBotsListInfo(12312312312312, 0);
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
    public class CsgoLeaderboardValidationTests
    {
        [TestMethod()]
        public void ShouldIncrementStatsTracker()
        {
            ulong testUserId = TestUser.TestUserId;

            CsgoLeaderboardManager.IncrementStatsCounter(testUserId, CsgoLeaderboardManager.StatItemType.Case, new ItemData()
            {
                WeaponType = WeaponType.Rifle,
                BlackListWeaponType = WeaponType.Knife,
                Rarity = Rarity.Covert
            });
            CsgoLeaderboardManager.IncrementStatsCounter(testUserId, CsgoLeaderboardManager.StatItemType.Case, new ItemData()
            {
                WeaponType = WeaponType.Knife,
                Rarity = Rarity.Covert
            });
            CsgoLeaderboardManager.IncrementStatsCounter(testUserId, CsgoLeaderboardManager.StatItemType.Drop, new ItemData()
            {
                WeaponType = WeaponType.Pistol,
                Rarity = Rarity.BaseGrade
            });
            CsgoLeaderboardManager.IncrementStatsCounter(testUserId, CsgoLeaderboardManager.StatItemType.Drop, new ItemData()
            {
                WeaponType = WeaponType.Rifle,
                Rarity = Rarity.MilSpecGrade
            });

            CsgoLeaderboardManager.IncrementStatsCounter(17923639926321631692, CsgoLeaderboardManager.StatItemType.Case, new ItemData()
            {
                WeaponType = WeaponType.Rifle,
                Rarity = Rarity.MilSpecGrade
            });
        }

        [TestMethod()]
        public void ShouldGetStatisticsLeader()
        {
            CsgoLeaderboardManager.GetStatisticsLeader(null);
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

    [TestClass()]
    public class FileManagerValidationTests
    {
        [TestMethod()]
        public void ShouldWriteToFileNoOverwrite()
        {
            string filePath = Directory.GetCurrentDirectory() + @"\noOverwritePls.txt";

            //Create the file
            using (StreamWriter file = new StreamWriter(filePath, true))
            {
                file.WriteLine("Line 1!!--");
            }

            //Write and hopefully overwrite
            FileManager.WriteStringToFile("overwrite", false, filePath);

            using (StreamReader r = new StreamReader(filePath))
            {
                if (!r.ReadToEnd().Contains("Line 1!!--"))
                {
                    File.Delete(filePath);
                    Assert.Fail("Overrode existing content");
                }
            }

            //Delete the test file
            File.Delete(filePath);
        }

        [TestMethod()]
        public void ShouldWriteToFileOverwrite()
        {
            string filePath = Directory.GetCurrentDirectory() + @"\overwriteThis.txt";

            //Create the file
            using (StreamWriter file = new StreamWriter(filePath, true))
            {
                file.WriteLine("Line 1!!--");
            }

            //Write and hopefully overwrite
            FileManager.WriteStringToFile("overwrite", true, filePath);

            using (StreamReader r = new StreamReader(filePath))
            {
                if (r.ReadToEnd().Contains("Line 1!!--"))
                {
                    File.Delete(filePath);
                    Assert.Fail("Did not overwrite existing content");
                }
            }

            //Delete the test file
            File.Delete(filePath);
        }

        [TestMethod()]
        public void ShouldNotReadFromFile()
        {
            FileManager.ReadFromFile(null);
        }

        [TestMethod()]
        public void ShouldReadFromFile()
        {
            //Make sure the paths file exists!
            string result = FileManager.ReadFromFile(Directory.GetCurrentDirectory() + @"\Paths.txt");

            if (string.IsNullOrWhiteSpace(result)) Assert.Fail("Failed to read from file");
        }

        [TestMethod()]
        public void ShouldCreateFileAndRead()
        {
            string filePath = Directory.GetCurrentDirectory() + @"\aNon-existantFile.txt";

            string result = FileManager.ReadFromFile(filePath);

            if (result != "")
            {
                Assert.Fail("Did not read file");
            }

            if (!File.Exists(filePath))
            {
                Assert.Fail("Did not create file");
            }

            //Delete the test file
            File.Delete(filePath);
        }

        [TestMethod()]
        public void ShouldGetFileLocation()
        {
            FileManager.GetFileLocation("anonexistantfile.txtxt");
            FileManager.GetFileLocation(null);

            string[] fileLocations = File.ReadAllLines(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Paths.txt");

            if (FileManager.GetFileLocation("selectedCases.json") != fileLocations.FirstOrDefault() + "selectedCases.json")
            {
                Assert.Fail("Found file location does not line up");
            }
        }
    }

    [TestClass()]
    public class UserInteractionValidationTests
    {
        [TestMethod()]
        public void ShouldBoldUserName()
        {
            if (UserInteraction.BoldUserName("This quick brown fox jumped over the lazy dog#1234") !=
                "**This quick brown fox jumped over the lazy dog**")
            {
                Assert.Fail();
            }

            UserInteraction.BoldUserName(null);
            UserInteraction.BoldUserName("aaaaaaaaa6666666666aaaaaaaaaaaaaaaaaaaaaaaa r2 12dqefgqwludguwgdkwrrrrrrrrrrrrrrrrrrrrrrrrraaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaattttttttttttttttttttthhhhhhhhhhhhhhhhhhhh");
        }

        [TestMethod()]
        public void ShouldReturnUserName()
        {
            if (UserInteraction.UserName("This quick brown fox jumped over the lazy dog#1234") !=
                "This quick brown fox jumped over the lazy dog")
            {
                Assert.Fail();
            }
            UserInteraction.UserName(null);
            UserInteraction.UserName("aaaaaaaaa6666666666aaaaaaaaaaaaaaaaaaaaaaaa r2 12dqefgqwludguwgdkwrrrrrrrrrrrrrrrrrrrrrrrrraaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaattttttttttttttttttttthhhhhhhhhhhhhhhhhhhh");
        }
    }

    public class TestUser
    {
        public static ulong TestUserId = 502245991953727490;
    }
}