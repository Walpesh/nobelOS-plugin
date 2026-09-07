using System.Collections.Generic;
using UnityEngine;

namespace StoryEngine.Core
{
    [CreateAssetMenu(fileName = "LocalizationCsv", menuName = "StoryEngine/Localization CSV", order = 90)]
    public class LocalizationCsvAsset : ScriptableObject, ILocalizationProvider
    {
        [SerializeField] private TextAsset csvFile;

        private readonly Dictionary<string, string> _map = new Dictionary<string, string>();
        private TextAsset _parsedFile;

        public string GetLocalizedText(string key)
        {
            EnsureParsed();
            return _map.TryGetValue(key, out var value) ? value : null;
        }

        private void EnsureParsed()
        {
            if (_parsedFile == csvFile && _map.Count > 0) return;
            if (csvFile == null) return;

            _parsedFile = csvFile;
            _map.Clear();

            using var reader = new System.IO.StringReader(csvFile.text);
            string line;
            bool firstLine = true;

            while ((line = reader.ReadLine()) != null)
            {
                if (firstLine)
                {
                    firstLine = false;
                    if (line.StartsWith("key", System.StringComparison.OrdinalIgnoreCase))
                        continue;
                }

                if (string.IsNullOrWhiteSpace(line)) continue;

                int separator = IndexOfSeparator(line);
                if (separator <= 0) continue;

                var key = line.Substring(0, separator).Trim();
                var value = line.Substring(separator + 1).Trim().Replace("\\n", "\n");

                if (!string.IsNullOrEmpty(key))
                    _map[key] = value;
            }
        }

        private static int IndexOfSeparator(string line)
        {
            int semi = line.IndexOf(';');
            int tab = line.IndexOf('\t');
            int comma = line.IndexOf(',');

            int best = -1;
            if (semi >= 0) best = semi;
            if (tab >= 0 && (best < 0 || tab < best)) best = tab;
            if (comma >= 0 && (best < 0 || comma < best)) best = comma;
            return best;
        }
    }
}