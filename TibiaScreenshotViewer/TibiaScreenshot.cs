using System;
using System.Globalization;

namespace TibiaScreenshotViewer
{
    public enum TibiaScreenshotType
    {
        Achievement,
        BestiaryEntryUnlocked,
        BestiaryEntryCompleted,
        BossDefeated,
        DeathPvE,
        DeathPvP,
        HighestDamageDealt,
        HighestHealingDone,
        LevelUp,
        LowHealth,
        PlayerAttacking,
        PlayerKill,
        PlayerKillAssist,
        SkillUp,
        TreasureFound,
        ValuableLoot,
        Unknown,
    }

    public class TibiaScreenshot
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public readonly string Path;
        public readonly string File;
        public readonly DateTime Timestamp;
        public readonly string Character;
        public readonly TibiaScreenshotType Type;

        public TibiaScreenshot(string path)
        {
            Path = path;

            File = System.IO.Path.GetFileNameWithoutExtension(Path);
            if (File == null)
                throw new Exception($"Could not extract file from path: {path}");

            var parts = File.Split('_');

            Timestamp = DateTime.ParseExact($"{parts[0]} {parts[1]}", "yyyy-MM-dd HHmmssfff", CultureInfo.InvariantCulture);

            Character = parts[2];

            Type = StringToType(parts[3]);
        }

        private TibiaScreenshotType StringToType(string str)
        {
            if (Enum.TryParse(str, out TibiaScreenshotType type))
                return type;

            Log.Warn($"Could not parse type string: {str}, unknown type for file {Path}");

            type = TibiaScreenshotType.Unknown;
            
            return type;
        }

        public string TypeToDisplayString()
        {
            return TypeToDisplayString(Type);
        }

        public static string TypeToDisplayString(TibiaScreenshotType type)
        {
            string label;

            switch (type)
            {
                case TibiaScreenshotType.Achievement:
                    label = "Achievement";
                    break;
                case TibiaScreenshotType.BestiaryEntryUnlocked:
                    label = "Bestiary Entry Unlocked";
                    break;
                case TibiaScreenshotType.BestiaryEntryCompleted:
                    label = "Bestiary Entry Completed";
                    break;
                case TibiaScreenshotType.BossDefeated:
                    label = "Boss Defeated";
                    break;
                case TibiaScreenshotType.DeathPvE:
                    label = "Death PvE";
                    break;
                case TibiaScreenshotType.DeathPvP:
                    label = "Death PvP";
                    break;
                case TibiaScreenshotType.HighestDamageDealt:
                    label = "Highest Damage Dealt";
                    break;
                case TibiaScreenshotType.HighestHealingDone:
                    label = "Highest Healing Done";
                    break;
                case TibiaScreenshotType.LevelUp:
                    label = "Level Up";
                    break;
                case TibiaScreenshotType.LowHealth:
                    label = "Low Health";
                    break;
                case TibiaScreenshotType.PlayerAttacking:
                    label = "Player Attacking";
                    break;
                case TibiaScreenshotType.PlayerKill:
                    label = "Player Kill";
                    break;
                case TibiaScreenshotType.PlayerKillAssist:
                    label = "Player Kill Assist";
                    break;
                case TibiaScreenshotType.SkillUp:
                    label = "Skill Up";
                    break;
                case TibiaScreenshotType.TreasureFound:
                    label = "Treasure Found";
                    break;
                case TibiaScreenshotType.ValuableLoot:
                    label = "Valuable Loot";
                    break;
                case TibiaScreenshotType.Unknown:
                    label = "Unknown";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            return label;
        }


        public override string ToString()
        {
            return $"TibiaScreenshot({Path},{File},{Timestamp},{Character},{Type})";
        }
    }
}
