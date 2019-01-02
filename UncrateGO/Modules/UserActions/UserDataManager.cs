using Discord.Commands;
using Discord.WebSocket;
using UncrateGo.Core;
using UncrateGo.Models;
using Newtonsoft.Json;
using System;

namespace UncrateGo.Modules.UserActions
{
    public class UserDataManager
    {
        public static void CreateNewUserXmlEntry(SocketCommandContext context)
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

        public static void CreateNewUserXmlEntry(SocketGuildUser user)
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
            return JsonConvert.DeserializeObject<UserStorage>(json);
        }
        public static void WriteUserStorage(UserStorage userStorage)
        {
            string jsonToWrite = JsonConvert.SerializeObject(userStorage);
            FileAccessManager.WriteStringToFile(jsonToWrite, true, FileAccessManager.GetFileLocation("UserStorage.json"));
        }
    }
}
