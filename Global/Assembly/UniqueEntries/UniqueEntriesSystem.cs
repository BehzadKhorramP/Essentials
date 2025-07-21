using System.Collections.Generic;
using System;

namespace MadApper
{
    public class UniqueEntriesSystem
    {
        public static bool Exists(string key) => DataBase.Exists(key);
        public static void Add(string key) => DataBase.Add(key);
        public static void Remove(string key) => DataBase.Remove(key);

        [Serializable]
        public class SaveData : ISaveable
        {
            const string name = "UniqueEntries";
            public string SaveName => name;

            public List<string> Entries;
            public SaveData()
            {
                Entries = new List<string>();
            }
            public string Hash { get; set; }

            public bool IgnoreHash => false;

        }
        public class DataBase : DataBase<SaveData>
        {
            public static bool Exists(string key) => Data.Entries.Contains(key);
            public static void Add(string key)
            {
                if (string.IsNullOrEmpty(key))
                    return;

                if (Exists(key))
                    return;

                Data.Entries.Add(key);

                Save();
            }
            public static void Remove(string key)
            {
                if (string.IsNullOrEmpty(key))
                    return;

                if (!Exists(key))
                    return;

                Data.Entries.Remove(key);

                Save();
            }
        }
    }
}
