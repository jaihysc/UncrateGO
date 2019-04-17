using UncrateGo.Modules.Csgo;
using System;
using System.Collections.Generic;

namespace UncrateGo.Models
{
    public class ItemListType
    {
        public Rarity Rarity { get; set; }
        public WeaponType? WeaponType { get; set; }
        public WeaponType? BlackListWeaponType { get; set; }
    }


    public class UserSkinStorage
    {
        public List<UserSkinEntry> UserSkinEntries { get; set; }
    }
    public class UserSkinEntry
    {
        public ulong OwnerId { get; set; }
        public string ClassId { get; set; }
        public DateTime UnboxDate { get; set; }
        public string MarketName { get; set; }
    }
}
