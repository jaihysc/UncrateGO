using Discord.Commands;
using Discord.WebSocket;
using UncrateGo.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;

namespace UncrateGo.Core
{
    public class UserDataManager
    {
        private static UserStorage _userStorage;

        public static void CreateNewUserEntry(SocketCommandContext context)
        {
            _userStorage.UserInfo.Add(context.Message.Author.Id, new UserInfo
            {
                UserId = context.Message.Author.Id,
                UserBankingStorage = new UserBankingStorage { Credit = 0},
            });
        }

        public static void CreateNewUserEntry(SocketGuildUser user)
        {
            _userStorage.UserInfo.Add(user.Id, new UserInfo
            {
                UserId = user.Id,
                UserBankingStorage = new UserBankingStorage { Credit = 0},
            });
        }

        /// <summary>
        /// Gets the user storage data
        /// </summary>
        /// <returns></returns>
        public static UserStorage GetUserStorage()
        {
            if (_userStorage == null)
            {
                var json = FileAccessManager.ReadFromFile(FileAccessManager.GetFileLocation("UserStorage.json"));
                var deserializedUserStorage = JsonConvert.DeserializeObject<UserStorage>(json);

                //In case the user storage file is blank
                if (deserializedUserStorage == null)
                {
                    deserializedUserStorage = new UserStorage
                    {
                        UserInfo = new Dictionary<ulong, UserInfo>()
                    };
                }

                _userStorage = deserializedUserStorage;
            }

            return _userStorage;
        }

        /// <summary>
        /// Sets the current userStorage to the input
        /// </summary>
        /// <param name="input"></param>
        public static void SetUserStorage(UserStorage input)
        {
            _userStorage = input;
        }

        /// <summary>
        /// Writes current userStorage to file
        /// </summary>
        public static void FlushUserStorage()
        {
            try
            {
                //Create a copy to write so that we don't get an error if it was modified mid write
                var tempUserStorage = _userStorage;
                string jsonToWrite = JsonConvert.SerializeObject(tempUserStorage);
                FileAccessManager.WriteStringToFile(jsonToWrite, true, FileAccessManager.GetFileLocation("UserStorage.json"));
            }
            catch (Exception)
            {
                EventLogger.LogMessage("Unable to flush user storage", ConsoleColor.Red);
            }
        }
    }
}
