using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;

namespace UncrateGo.Core
{
    public static class UserDataManager
    {
        private static UserStorage _userStorage;

        public static void CreateNewUserEntry(SocketCommandContext context)
        {
            _userStorage.UserInfo.Add(context.Message.Author.Id, new UserInfo
            {
                UserId = context.Message.Author.Id,
                UserCredit = 0,
            });
        }

        public static void CreateNewUserEntry(SocketGuildUser user)
        {
            _userStorage.UserInfo.Add(user.Id, new UserInfo
            {
                UserId = user.Id,
                UserCredit = 0,
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
                    EventLogger.LogMessage("UserStorage.json not found, creating one", EventLogger.LogLevel.Info);

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
        /// Get user credits
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static long GetUserCredit(ulong userId)
        {
            var userStorage = GetUserStorage();

            if (userStorage.UserInfo.TryGetValue(userId, out var userVal))
            {
                return userVal.UserCredit;
            }

            return -1;
        }

        /// <summary>
        /// Get user credits
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static UserCsgoStatsStorage GetUserCsgoStatsStorage(ulong userId)
        {
            var userStorage = GetUserStorage();

            if (userStorage.UserInfo.TryGetValue(userId, out var userVal))
            {
                return userVal.UserCsgoStatsStorage;
            }

            return null;
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
        /// Sets specified user credits to input
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userCredit"></param>
        public static void SetUserCredit(ulong userId, long userCredit)
        {
            var userStorage = GetUserStorage();

            if (userStorage.UserInfo.TryGetValue(userId, out var user))
            {
                user.UserCredit = userCredit;

                //Set new credits amount 
                SetUserStorage(userStorage);
            }
        }

        public static void SetUserCsgoStatsStorage(ulong userId, UserCsgoStatsStorage userCsgoStatsStorage)
        {
            var userStorage = GetUserStorage();

            if (userStorage.UserInfo.TryGetValue(userId, out var user))
            {
                user.UserCsgoStatsStorage = userCsgoStatsStorage;

                //Set new credits amount 
                SetUserStorage(userStorage);
            }
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
                EventLogger.LogMessage("Unable to flush user storage", EventLogger.LogLevel.Error);
            }
        }
    }

    public class UserStorage
    {
        public Dictionary<ulong, UserInfo> UserInfo { get; set; }
    }
    public class UserInfo
    {
        public ulong UserId { get; set; }
        public long UserCredit { get; set; }
        public UserCsgoStatsStorage UserCsgoStatsStorage { get; set; }
    }
    public class UserCsgoStatsStorage
    {
        public long CasesOpened { get; set; }
        public long SouvenirsOpened { get; set; }
        public long DropsOpened { get; set; }
        public long StickersOpened { get; set; }

        public long ConsumerGrade { get; set; }
        public long IndustrialGrade { get; set; }
        public long MilSpecGrade { get; set; }
        public long Restricted { get; set; }
        public long Classified { get; set; }
        public long Covert { get; set; }
        public long Special { get; set; }
        public long Stickers { get; set; }
        public long Other { get; set; }
    }
}
