using Discord.Commands;
using Discord.WebSocket;
using UncrateGo.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace UncrateGo.Core
{
    public class UserDataManager
    {
        public static void CreateNewUserEntry(SocketCommandContext context)
        {
            var userStorage = GetUserStorage();

            userStorage.UserInfo.Add(context.Message.Author.Id, new UserInfo
            {
                UserId = context.Message.Author.Id,
                UserBankingStorage = new UserBankingStorage { Credit = 0, CreditDebt = 0 },
            });

            var userRecord = new UserStorage
            {
                UserInfo = userStorage.UserInfo
            };


            WriteUserStorage(userRecord);
        }

        public static void CreateNewUserEntry(SocketGuildUser user)
        {
            var userStorage = GetUserStorage();

            userStorage.UserInfo.Add(user.Id, new UserInfo
            {
                UserId = user.Id,
                UserBankingStorage = new UserBankingStorage { Credit = 0, CreditDebt = 0 },
            });

            var userRecord = new UserStorage
            {
                UserInfo = userStorage.UserInfo
            };


            WriteUserStorage(userRecord);
        }

        public static UserStorage GetUserStorage()
        {
            var json = FileAccessManager.ReadFromFile(FileAccessManager.GetFileLocation("UserStorage.json"));
            var deserialized = JsonConvert.DeserializeObject<UserStorage>(json);

            //In case the user storage file is blank
            if (deserialized == null)
            {
                var newUserStorage = new UserStorage
                {
                    UserInfo = new Dictionary<ulong, UserInfo>()
                };

                return newUserStorage;
            }

            return deserialized;
        }
        public static void WriteUserStorage(UserStorage userStorage)
        {
            string jsonToWrite = JsonConvert.SerializeObject(userStorage);
            FileAccessManager.WriteStringToFile(jsonToWrite, true, FileAccessManager.GetFileLocation("UserStorage.json"));
        }
    }
}
