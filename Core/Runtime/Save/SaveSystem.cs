using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace StoryEngine.Core
{
    [Serializable]
    public class AttributeSaveEntry
    {
        public string id;
        public float number;
        public bool boolValue;
        public string stringValue;
    }

    [Serializable]
    public class SaveData
    {
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;
        public string timestamp = string.Empty;
        public string graphId = string.Empty;
        public string nodeId = string.Empty;
        public List<AttributeSaveEntry> attributes = new List<AttributeSaveEntry>();
        public List<string> visitedNodes = new List<string>();
        public List<string> madeChoices = new List<string>();
    }

    [Serializable]
    public class EndingStatEntry
    {
        public string endingId;
        public int count;
    }

    [Serializable]
    public class ChoiceStatEntry
    {
        public string choiceId;
        public int count;
    }

    [Serializable]
    public class MetaProfile
    {
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;
        public int playthroughs;
        public List<string> unlockedEndings = new List<string>();
        public List<EndingStatEntry> endingStats = new List<EndingStatEntry>();
        public List<ChoiceStatEntry> choiceStats = new List<ChoiceStatEntry>();
    }

    public static class SaveSystem
    {
        public const int SlotCount = 3;

        private static MetaProfile _profileCache;

        private static string SavesDirectory => Path.Combine(Application.persistentDataPath, "StoryEngine", "saves");
        private static string ProfilePath => Path.Combine(Application.persistentDataPath, "StoryEngine", "profile.json");

        private static string SlotPath(int slot) => Path.Combine(SavesDirectory, $"slot_{slot}.json");

        public static void SaveSlot(int slot, SaveData data)
        {
            if (data == null) return;
            Directory.CreateDirectory(SavesDirectory);
            File.WriteAllText(SlotPath(slot), JsonUtility.ToJson(data, true));
        }

        public static SaveData LoadSlot(int slot)
        {
            var path = SlotPath(slot);
            if (!File.Exists(path)) return null;

            try
            {
                return JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
            }
            catch (Exception exception)
            {
                Debug.LogError($"[SaveSystem] Failed to load slot {slot}: {exception.Message}");
                return null;
            }
        }

        public static bool HasSlot(int slot) => File.Exists(SlotPath(slot));

        public static void DeleteSlot(int slot)
        {
            var path = SlotPath(slot);
            if (File.Exists(path)) File.Delete(path);
        }

        public static string GetSlotTimestamp(int slot)
        {
            var data = LoadSlot(slot);
            return data != null ? data.timestamp : string.Empty;
        }

        // ------------------------------------------------------------------
        // Meta profile
        // ------------------------------------------------------------------

        public static MetaProfile LoadProfile(bool forceReload = false)
        {
            if (_profileCache != null && !forceReload) return _profileCache;

            if (File.Exists(ProfilePath))
            {
                try
                {
                    _profileCache = JsonUtility.FromJson<MetaProfile>(File.ReadAllText(ProfilePath));
                }
                catch (Exception exception)
                {
                    Debug.LogError($"[SaveSystem] Failed to load profile: {exception.Message}");
                    _profileCache = null;
                }
            }

            if (_profileCache == null) _profileCache = new MetaProfile();
            return _profileCache;
        }

        public static void SaveProfile()
        {
            if (_profileCache == null) return;
            Directory.CreateDirectory(Path.GetDirectoryName(ProfilePath));
            File.WriteAllText(ProfilePath, JsonUtility.ToJson(_profileCache, true));
        }

        public static void RecordPlaythrough()
        {
            var profile = LoadProfile();
            profile.playthroughs++;
            SaveProfile();
        }

        public static void RecordChoice(string choiceId)
        {
            if (string.IsNullOrEmpty(choiceId)) return;

            var profile = LoadProfile();
            var entry = FindChoiceEntry(profile, choiceId);
            entry.count++;
            SaveProfile();
        }

        public static void UnlockEnding(string endingId)
        {
            if (string.IsNullOrEmpty(endingId)) return;

            var profile = LoadProfile();

            if (!profile.unlockedEndings.Contains(endingId))
                profile.unlockedEndings.Add(endingId);

            var entry = FindEndingEntry(profile, endingId);
            entry.count++;
            SaveProfile();
        }

        public static int GetChoiceCount(string choiceId)
        {
            var profile = LoadProfile();
            return FindChoiceEntry(profile, choiceId).count;
        }

        public static int GetEndingCount(string endingId)
        {
            var profile = LoadProfile();
            return FindEndingEntry(profile, endingId).count;
        }

        private static ChoiceStatEntry FindChoiceEntry(MetaProfile profile, string choiceId)
        {
            for (int i = 0; i < profile.choiceStats.Count; ++i)
            {
                if (profile.choiceStats[i].choiceId == choiceId)
                    return profile.choiceStats[i];
            }

            var created = new ChoiceStatEntry { choiceId = choiceId, count = 0 };
            profile.choiceStats.Add(created);
            return created;
        }

        private static EndingStatEntry FindEndingEntry(MetaProfile profile, string endingId)
        {
            for (int i = 0; i < profile.endingStats.Count; ++i)
            {
                if (profile.endingStats[i].endingId == endingId)
                    return profile.endingStats[i];
            }

            var created = new EndingStatEntry { endingId = endingId, count = 0 };
            profile.endingStats.Add(created);
            return created;
        }
    }
}