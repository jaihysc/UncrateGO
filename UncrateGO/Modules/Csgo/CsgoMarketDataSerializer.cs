using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UncrateGo.Modules.Csgo
{
    public partial class RootSkinData
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("processed")]
        public bool Processed { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("items_list")]
        public Dictionary<string, SkinDataItem> ItemsList { get; set; }
    }

    public partial class SkinDataItem
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("cases")]
        public List<Case> Cases { get; set; }

        [JsonProperty("marketable")]
        public long Marketable { get; set; }

        [JsonProperty("tradable")]
        public long Tradable { get; set; }

        [JsonProperty("classid")]
        public string Classid { get; set; }

        [JsonProperty("icon_url")]
        public string IconUrl { get; set; }

        [JsonProperty("icon_url_large")]
        public string IconUrlLarge { get; set; }

        [JsonProperty("type")]
        public TypeEnum? Type { get; set; }

        [JsonProperty("weapon_type", NullValueHandling = NullValueHandling.Ignore)]
        public WeaponType? WeaponType { get; set; }

        [JsonProperty("gun_type", NullValueHandling = NullValueHandling.Ignore)]
        public GunType? GunType { get; set; }

        [JsonProperty("exterior", NullValueHandling = NullValueHandling.Ignore)]
        public Exterior? Exterior { get; set; }

        [JsonProperty("rarity")]
        public Rarity Rarity { get; set; }

        [JsonProperty("rarity_color")]
        public string RarityColor { get; set; }

        [JsonProperty("price", NullValueHandling = NullValueHandling.Ignore)]
        public Price Price { get; set; }

        [JsonProperty("first_sale_date", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(ParseStringConverter))]
        public long? FirstSaleDate { get; set; }

        [JsonProperty("souvenir", NullValueHandling = NullValueHandling.Ignore)]
        public long? Souvenir { get; set; }

        [JsonProperty("tournament", NullValueHandling = NullValueHandling.Ignore)]
        public string Tournament { get; set; }

        [JsonProperty("stattrak", NullValueHandling = NullValueHandling.Ignore)]
        public long? Stattrak { get; set; }

        [JsonProperty("sticker", NullValueHandling = NullValueHandling.Ignore)]
        public long? Sticker { get; set; }

        [JsonProperty("knife_type", NullValueHandling = NullValueHandling.Ignore)]
        public KnifeType? KnifeType { get; set; }
    }

    public partial class Case
    {
        public string CaseName { get; set; }
        public string CaseCollection { get; set; }
    }

    public partial class Price
    {
        [JsonProperty("24_hours", NullValueHandling = NullValueHandling.Ignore)]
        public The24__Hours The24_Hours { get; set; }

        [JsonProperty("7_days", NullValueHandling = NullValueHandling.Ignore)]
        public The24__Hours The7_Days { get; set; }

        [JsonProperty("30_days", NullValueHandling = NullValueHandling.Ignore)]
        public The24__Hours The30_Days { get; set; }

        [JsonProperty("all_time")]
        public The24__Hours AllTime { get; set; }
    }

    public partial class The24__Hours
    {
        [JsonProperty("average")]
        public double Average { get; set; }

        [JsonProperty("median")]
        public double Median { get; set; }

        [JsonProperty("sold")]
        public string Sold { get; set; }

        [JsonProperty("standard_deviation")]
        public string StandardDeviation { get; set; }

        [JsonProperty("lowest_price")]
        public double LowestPrice { get; set; }

        [JsonProperty("highest_price")]
        public double HighestPrice { get; set; }
    }

    public enum Exterior { BattleScarred, FactoryNew, FieldTested, MinimalWear, NotPainted, WellWorn };

    public enum GunType { Ak47, Aug, Awp, Cz75Auto, DesertEagle, DualBerettas, Famas, FiveSeveN, G3Sg1, GalilAr, Glock18, M249, M4A1S, M4A4, Mac10, Mag7, Mp5Sd, Mp7, Mp9, Negev, Nova, P2000, P250, P90, PpBizon, R8Revolver, SawedOff, Scar20, Sg553, Ssg08, Tec9, Ump45, UspS, Xm1014 };

    public enum KnifeType { Bayonet, BowieKnife, ButterflyKnife, FalchionKnife, FlipKnife, GutKnife, HuntsmanKnife, Karambit, M9Bayonet, NavajaKnife, ShadowDaggers, StilettoKnife, TalonKnife, UrsusKnife };

    public enum Rarity { BaseGrade, Classified, ConsumerGrade, Contraband, Covert, Exotic, Extraordinary, HighGrade, IndustrialGrade, MilSpecGrade, Remarkable, Restricted };

    public enum TypeEnum { Collectible, Container, Gift, Gloves, Graffiti, Key, MusicKit, Pass, Weapon };

    public enum WeaponType { Knife, Machinegun, Pistol, Rifle, Shotgun, Smg, SniperRifle };

    public partial class RootSkinData
    {
        public static RootSkinData FromJson(string json) => JsonConvert.DeserializeObject<RootSkinData>(json, UncrateGo.Modules.Csgo.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this RootSkinData self) => JsonConvert.SerializeObject(self, UncrateGo.Modules.Csgo.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                ExteriorConverter.Singleton,
                GunTypeConverter.Singleton,
                KnifeTypeConverter.Singleton,
                RarityConverter.Singleton,
                TypeEnumConverter.Singleton,
                WeaponTypeConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class ExteriorConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Exterior) || t == typeof(Exterior?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "Battle-Scarred":
                    return Exterior.BattleScarred;
                case "Factory New":
                    return Exterior.FactoryNew;
                case "Field-Tested":
                    return Exterior.FieldTested;
                case "Minimal Wear":
                    return Exterior.MinimalWear;
                case "Not Painted":
                    return Exterior.NotPainted;
                case "Well-Worn":
                    return Exterior.WellWorn;

                //Number form

                case "0":
                    return Exterior.BattleScarred;
                case "1":
                    return Exterior.FactoryNew;
                case "2":
                    return Exterior.FieldTested;
                case "3":
                    return Exterior.MinimalWear;
                case "4":
                    return Exterior.NotPainted;
                case "5":
                    return Exterior.WellWorn;
            }
            Console.WriteLine("Exception - Cannot unmarshal type Exterior");
            return null;
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Exterior)untypedValue;
            switch (value)
            {
                case Exterior.BattleScarred:
                    serializer.Serialize(writer, "Battle-Scarred");
                    return;
                case Exterior.FactoryNew:
                    serializer.Serialize(writer, "Factory New");
                    return;
                case Exterior.FieldTested:
                    serializer.Serialize(writer, "Field-Tested");
                    return;
                case Exterior.MinimalWear:
                    serializer.Serialize(writer, "Minimal Wear");
                    return;
                case Exterior.NotPainted:
                    serializer.Serialize(writer, "Not Painted");
                    return;
                case Exterior.WellWorn:
                    serializer.Serialize(writer, "Well-Worn");
                    return;
            }
            Console.WriteLine("Exception - Cannot marshal type Exterior");
        }

        public static readonly ExteriorConverter Singleton = new ExteriorConverter();
    }

    internal class ParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            long l;
            if (Int64.TryParse(value, out l))
            {
                return l;
            }
            Console.WriteLine("Exception - Cannot unmarshal type long");
            return null;
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (long)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    }

    internal class GunTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(GunType) || t == typeof(GunType?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "AK-47":
                    return GunType.Ak47;
                case "AUG":
                    return GunType.Aug;
                case "AWP":
                    return GunType.Awp;
                case "CZ75-Auto":
                    return GunType.Cz75Auto;
                case "Desert Eagle":
                    return GunType.DesertEagle;
                case "Dual Berettas":
                    return GunType.DualBerettas;
                case "FAMAS":
                    return GunType.Famas;
                case "Five-SeveN":
                    return GunType.FiveSeveN;
                case "G3SG1":
                    return GunType.G3Sg1;
                case "Galil AR":
                    return GunType.GalilAr;
                case "Glock-18":
                    return GunType.Glock18;
                case "M249":
                    return GunType.M249;
                case "M4A1-S":
                    return GunType.M4A1S;
                case "M4A4":
                    return GunType.M4A4;
                case "MAC-10":
                    return GunType.Mac10;
                case "MAG-7":
                    return GunType.Mag7;
                case "MP5-SD":
                    return GunType.Mp5Sd;
                case "MP7":
                    return GunType.Mp7;
                case "MP9":
                    return GunType.Mp9;
                case "Negev":
                    return GunType.Negev;
                case "Nova":
                    return GunType.Nova;
                case "P2000":
                    return GunType.P2000;
                case "P250":
                    return GunType.P250;
                case "P90":
                    return GunType.P90;
                case "PP-Bizon":
                    return GunType.PpBizon;
                case "R8 Revolver":
                    return GunType.R8Revolver;
                case "SCAR-20":
                    return GunType.Scar20;
                case "SG 553":
                    return GunType.Sg553;
                case "SSG 08":
                    return GunType.Ssg08;
                case "Sawed-Off":
                    return GunType.SawedOff;
                case "Tec-9":
                    return GunType.Tec9;
                case "UMP-45":
                    return GunType.Ump45;
                case "USP-S":
                    return GunType.UspS;
                case "XM1014":
                    return GunType.Xm1014;

                //Number form
                case "0":
                    return GunType.Ak47;
                case "1":
                    return GunType.Aug;
                case "2":
                    return GunType.Awp;
                case "3":
                    return GunType.Cz75Auto;
                case "4":
                    return GunType.DesertEagle;
                case "5":
                    return GunType.DualBerettas;
                case "6":
                    return GunType.Famas;
                case "7":
                    return GunType.FiveSeveN;
                case "8":
                    return GunType.G3Sg1;
                case "9":
                    return GunType.GalilAr;
                case "10":
                    return GunType.Glock18;
                case "11":
                    return GunType.M249;
                case "12":
                    return GunType.M4A1S;
                case "13":
                    return GunType.M4A4;
                case "14":
                    return GunType.Mac10;
                case "15":
                    return GunType.Mag7;
                case "16":
                    return GunType.Mp5Sd;
                case "17":
                    return GunType.Mp7;
                case "18":
                    return GunType.Mp9;
                case "19":
                    return GunType.Negev;
                case "20":
                    return GunType.Nova;
                case "21":
                    return GunType.P2000;
                case "22":
                    return GunType.P250;
                case "23":
                    return GunType.P90;
                case "24":
                    return GunType.PpBizon;
                case "25":
                    return GunType.R8Revolver;
                case "26":
                    return GunType.Scar20;
                case "27":
                    return GunType.Sg553;
                case "28":
                    return GunType.Ssg08;
                case "29":
                    return GunType.SawedOff;
                case "30":
                    return GunType.Tec9;
                case "31":
                    return GunType.Ump45;
                case "32":
                    return GunType.UspS;
                case "33":
                    return GunType.Xm1014;
            }
            Console.WriteLine("Exception - Cannot unmarshal type GunType");
            return null;
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (GunType)untypedValue;
            switch (value)
            {
                case GunType.Ak47:
                    serializer.Serialize(writer, "AK-47");
                    return;
                case GunType.Aug:
                    serializer.Serialize(writer, "AUG");
                    return;
                case GunType.Awp:
                    serializer.Serialize(writer, "AWP");
                    return;
                case GunType.Cz75Auto:
                    serializer.Serialize(writer, "CZ75-Auto");
                    return;
                case GunType.DesertEagle:
                    serializer.Serialize(writer, "Desert Eagle");
                    return;
                case GunType.DualBerettas:
                    serializer.Serialize(writer, "Dual Berettas");
                    return;
                case GunType.Famas:
                    serializer.Serialize(writer, "FAMAS");
                    return;
                case GunType.FiveSeveN:
                    serializer.Serialize(writer, "Five-SeveN");
                    return;
                case GunType.G3Sg1:
                    serializer.Serialize(writer, "G3SG1");
                    return;
                case GunType.GalilAr:
                    serializer.Serialize(writer, "Galil AR");
                    return;
                case GunType.Glock18:
                    serializer.Serialize(writer, "Glock-18");
                    return;
                case GunType.M249:
                    serializer.Serialize(writer, "M249");
                    return;
                case GunType.M4A1S:
                    serializer.Serialize(writer, "M4A1-S");
                    return;
                case GunType.M4A4:
                    serializer.Serialize(writer, "M4A4");
                    return;
                case GunType.Mac10:
                    serializer.Serialize(writer, "MAC-10");
                    return;
                case GunType.Mag7:
                    serializer.Serialize(writer, "MAG-7");
                    return;
                case GunType.Mp5Sd:
                    serializer.Serialize(writer, "MP5-SD");
                    return;
                case GunType.Mp7:
                    serializer.Serialize(writer, "MP7");
                    return;
                case GunType.Mp9:
                    serializer.Serialize(writer, "MP9");
                    return;
                case GunType.Negev:
                    serializer.Serialize(writer, "Negev");
                    return;
                case GunType.Nova:
                    serializer.Serialize(writer, "Nova");
                    return;
                case GunType.P2000:
                    serializer.Serialize(writer, "P2000");
                    return;
                case GunType.P250:
                    serializer.Serialize(writer, "P250");
                    return;
                case GunType.P90:
                    serializer.Serialize(writer, "P90");
                    return;
                case GunType.PpBizon:
                    serializer.Serialize(writer, "PP-Bizon");
                    return;
                case GunType.R8Revolver:
                    serializer.Serialize(writer, "R8 Revolver");
                    return;
                case GunType.Scar20:
                    serializer.Serialize(writer, "SCAR-20");
                    return;
                case GunType.Sg553:
                    serializer.Serialize(writer, "SG 553");
                    return;
                case GunType.Ssg08:
                    serializer.Serialize(writer, "SSG 08");
                    return;
                case GunType.SawedOff:
                    serializer.Serialize(writer, "Sawed-Off");
                    return;
                case GunType.Tec9:
                    serializer.Serialize(writer, "Tec-9");
                    return;
                case GunType.Ump45:
                    serializer.Serialize(writer, "UMP-45");
                    return;
                case GunType.UspS:
                    serializer.Serialize(writer, "USP-S");
                    return;
                case GunType.Xm1014:
                    serializer.Serialize(writer, "XM1014");
                    return;
            }
            Console.WriteLine("Exception - Cannot marshal type GunType");
        }

        public static readonly GunTypeConverter Singleton = new GunTypeConverter();
    }

    internal class KnifeTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(KnifeType) || t == typeof(KnifeType?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "Bayonet":
                    return KnifeType.Bayonet;
                case "Bowie Knife":
                    return KnifeType.BowieKnife;
                case "Butterfly Knife":
                    return KnifeType.ButterflyKnife;
                case "Falchion Knife":
                    return KnifeType.FalchionKnife;
                case "Flip Knife":
                    return KnifeType.FlipKnife;
                case "Gut Knife":
                    return KnifeType.GutKnife;
                case "Huntsman Knife":
                    return KnifeType.HuntsmanKnife;
                case "Karambit":
                    return KnifeType.Karambit;
                case "M9 Bayonet":
                    return KnifeType.M9Bayonet;
                case "Navaja Knife":
                    return KnifeType.NavajaKnife;
                case "Shadow Daggers":
                    return KnifeType.ShadowDaggers;
                case "Stiletto Knife":
                    return KnifeType.StilettoKnife;
                case "Talon Knife":
                    return KnifeType.TalonKnife;
                case "Ursus Knife":
                    return KnifeType.UrsusKnife;

                //Number form

                case "0":
                    return KnifeType.Bayonet;
                case "1":
                    return KnifeType.BowieKnife;
                case "2":
                    return KnifeType.ButterflyKnife;
                case "3":
                    return KnifeType.FalchionKnife;
                case "4":
                    return KnifeType.FlipKnife;
                case "5":
                    return KnifeType.GutKnife;
                case "6":
                    return KnifeType.HuntsmanKnife;
                case "7":
                    return KnifeType.Karambit;
                case "8":
                    return KnifeType.M9Bayonet;
                case "9":
                    return KnifeType.NavajaKnife;
                case "10":
                    return KnifeType.ShadowDaggers;
                case "11":
                    return KnifeType.StilettoKnife;
                case "12":
                    return KnifeType.TalonKnife;
                case "13":
                    return KnifeType.UrsusKnife;
            }
            Console.WriteLine("Exception - Cannot unmarshal type KnifeType");
            return null;
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (KnifeType)untypedValue;
            switch (value)
            {
                case KnifeType.Bayonet:
                    serializer.Serialize(writer, "Bayonet");
                    return;
                case KnifeType.BowieKnife:
                    serializer.Serialize(writer, "Bowie Knife");
                    return;
                case KnifeType.ButterflyKnife:
                    serializer.Serialize(writer, "Butterfly Knife");
                    return;
                case KnifeType.FalchionKnife:
                    serializer.Serialize(writer, "Falchion Knife");
                    return;
                case KnifeType.FlipKnife:
                    serializer.Serialize(writer, "Flip Knife");
                    return;
                case KnifeType.GutKnife:
                    serializer.Serialize(writer, "Gut Knife");
                    return;
                case KnifeType.HuntsmanKnife:
                    serializer.Serialize(writer, "Huntsman Knife");
                    return;
                case KnifeType.Karambit:
                    serializer.Serialize(writer, "Karambit");
                    return;
                case KnifeType.M9Bayonet:
                    serializer.Serialize(writer, "M9 Bayonet");
                    return;
                case KnifeType.NavajaKnife:
                    serializer.Serialize(writer, "Navaja Knife");
                    return;
                case KnifeType.ShadowDaggers:
                    serializer.Serialize(writer, "Shadow Daggers");
                    return;
                case KnifeType.StilettoKnife:
                    serializer.Serialize(writer, "Stiletto Knife");
                    return;
                case KnifeType.TalonKnife:
                    serializer.Serialize(writer, "Talon Knife");
                    return;
                case KnifeType.UrsusKnife:
                    serializer.Serialize(writer, "Ursus Knife");
                    return;
            }
            Console.WriteLine("Exception - Cannot marshal type KnifeType");
        }

        public static readonly KnifeTypeConverter Singleton = new KnifeTypeConverter();
    }

    internal class RarityConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Rarity) || t == typeof(Rarity?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "Base Grade":
                    return Rarity.BaseGrade;
                case "Classified":
                    return Rarity.Classified;
                case "Consumer Grade":
                    return Rarity.ConsumerGrade;
                case "Contraband":
                    return Rarity.Contraband;
                case "Covert":
                    return Rarity.Covert;
                case "Exotic":
                    return Rarity.Exotic;
                case "Extraordinary":
                    return Rarity.Extraordinary;
                case "High Grade":
                    return Rarity.HighGrade;
                case "Industrial Grade":
                    return Rarity.IndustrialGrade;
                case "Mil-Spec Grade":
                    return Rarity.MilSpecGrade;
                case "Remarkable":
                    return Rarity.Remarkable;
                case "Restricted":
                    return Rarity.Restricted;

                //Numbe form

                case "0":
                    return Rarity.BaseGrade;
                case "1":
                    return Rarity.Classified;
                case "2":
                    return Rarity.ConsumerGrade;
                case "3":
                    return Rarity.Contraband;
                case "4":
                    return Rarity.Covert;
                case "5":
                    return Rarity.Exotic;
                case "6":
                    return Rarity.Extraordinary;
                case "7":
                    return Rarity.HighGrade;
                case "8":
                    return Rarity.IndustrialGrade;
                case "9":
                    return Rarity.MilSpecGrade;
                case "10":
                    return Rarity.Remarkable;
                case "11":
                    return Rarity.Restricted;
            }
            Console.WriteLine("Exception - Cannot unmarshal type Rarity");
            return null;
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Rarity)untypedValue;
            switch (value)
            {
                case Rarity.BaseGrade:
                    serializer.Serialize(writer, "Base Grade");
                    return;
                case Rarity.Classified:
                    serializer.Serialize(writer, "Classified");
                    return;
                case Rarity.ConsumerGrade:
                    serializer.Serialize(writer, "Consumer Grade");
                    return;
                case Rarity.Contraband:
                    serializer.Serialize(writer, "Contraband");
                    return;
                case Rarity.Covert:
                    serializer.Serialize(writer, "Covert");
                    return;
                case Rarity.Exotic:
                    serializer.Serialize(writer, "Exotic");
                    return;
                case Rarity.Extraordinary:
                    serializer.Serialize(writer, "Extraordinary");
                    return;
                case Rarity.HighGrade:
                    serializer.Serialize(writer, "High Grade");
                    return;
                case Rarity.IndustrialGrade:
                    serializer.Serialize(writer, "Industrial Grade");
                    return;
                case Rarity.MilSpecGrade:
                    serializer.Serialize(writer, "Mil-Spec Grade");
                    return;
                case Rarity.Remarkable:
                    serializer.Serialize(writer, "Remarkable");
                    return;
                case Rarity.Restricted:
                    serializer.Serialize(writer, "Restricted");
                    return;
            }
            Console.WriteLine("Exception - Cannot marshal type Rarity");
        }

        public static readonly RarityConverter Singleton = new RarityConverter();
    }

    internal class TypeEnumConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(TypeEnum) || t == typeof(TypeEnum?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "Collectible":
                    return TypeEnum.Collectible;
                case "Container":
                    return TypeEnum.Container;
                case "Gift":
                    return TypeEnum.Gift;
                case "Gloves":
                    return TypeEnum.Gloves;
                case "Graffiti":
                    return TypeEnum.Graffiti;
                case "Key":
                    return TypeEnum.Key;
                case "Music Kit":
                    return TypeEnum.MusicKit;
                case "Pass":
                    return TypeEnum.Pass;
                case "Weapon":
                    return TypeEnum.Weapon;

                    //In number form after it is converted
                case "0":
                    return TypeEnum.Collectible;
                case "1":
                    return TypeEnum.Container;
                case "2":
                    return TypeEnum.Gift;
                case "3":
                    return TypeEnum.Gloves;
                case "4":
                    return TypeEnum.Graffiti;
                case "5":
                    return TypeEnum.Key;
                case "6":
                    return TypeEnum.MusicKit;
                case "7":
                    return TypeEnum.Pass;
                case "8":
                    return TypeEnum.Weapon;
            }
            Console.WriteLine("Exception - Cannot unmarshal type TypeEnum");
            return null;
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (TypeEnum)untypedValue;
            switch (value)
            {
                case TypeEnum.Collectible:
                    serializer.Serialize(writer, "Collectible");
                    return;
                case TypeEnum.Container:
                    serializer.Serialize(writer, "Container");
                    return;
                case TypeEnum.Gift:
                    serializer.Serialize(writer, "Gift");
                    return;
                case TypeEnum.Gloves:
                    serializer.Serialize(writer, "Gloves");
                    return;
                case TypeEnum.Graffiti:
                    serializer.Serialize(writer, "Graffiti");
                    return;
                case TypeEnum.Key:
                    serializer.Serialize(writer, "Key");
                    return;
                case TypeEnum.MusicKit:
                    serializer.Serialize(writer, "Music Kit");
                    return;
                case TypeEnum.Pass:
                    serializer.Serialize(writer, "Pass");
                    return;
                case TypeEnum.Weapon:
                    serializer.Serialize(writer, "Weapon");
                    return;
            }
            Console.WriteLine("Exception - Cannot marshal type TypeEnum");
        }

        public static readonly TypeEnumConverter Singleton = new TypeEnumConverter();
    }

    internal class WeaponTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(WeaponType) || t == typeof(WeaponType?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "Knife":
                    return WeaponType.Knife;
                case "Machinegun":
                    return WeaponType.Machinegun;
                case "Pistol":
                    return WeaponType.Pistol;
                case "Rifle":
                    return WeaponType.Rifle;
                case "SMG":
                    return WeaponType.Smg;
                case "Shotgun":
                    return WeaponType.Shotgun;
                case "Sniper Rifle":
                    return WeaponType.SniperRifle;

                //Number form

                case "0":
                    return WeaponType.Knife;
                case "1":
                    return WeaponType.Machinegun;
                case "2":
                    return WeaponType.Pistol;
                case "3":
                    return WeaponType.Rifle;
                case "4":
                    return WeaponType.Smg;
                case "5":
                    return WeaponType.Shotgun;
                case "6":
                    return WeaponType.SniperRifle;
            }
            Console.WriteLine("Exception - Cannot unmarshal type WeaponType");
            return null;
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (WeaponType)untypedValue;
            switch (value)
            {
                case WeaponType.Knife:
                    serializer.Serialize(writer, "Knife");
                    return;
                case WeaponType.Machinegun:
                    serializer.Serialize(writer, "Machinegun");
                    return;
                case WeaponType.Pistol:
                    serializer.Serialize(writer, "Pistol");
                    return;
                case WeaponType.Rifle:
                    serializer.Serialize(writer, "Rifle");
                    return;
                case WeaponType.Smg:
                    serializer.Serialize(writer, "SMG");
                    return;
                case WeaponType.Shotgun:
                    serializer.Serialize(writer, "Shotgun");
                    return;
                case WeaponType.SniperRifle:
                    serializer.Serialize(writer, "Sniper Rifle");
                    return;
            }
            Console.WriteLine("Exception - Cannot marshal type WeaponType");
        }

        public static readonly WeaponTypeConverter Singleton = new WeaponTypeConverter();
    }
}
