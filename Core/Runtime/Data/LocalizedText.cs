using System;
using UnityEngine;

namespace StoryEngine.Core
{
    public interface ILocalizationProvider
    {
        string GetLocalizedText(string key);
    }

    [Serializable]
    public struct LocalizedText
    {
        public string key;

        [TextArea(2, 5)]
        public string fallback;

        public string Get(ILocalizationProvider provider)
        {
            if (!string.IsNullOrEmpty(key) && provider != null)
            {
                var localized = provider.GetLocalizedText(key);
                if (!string.IsNullOrEmpty(localized))
                    return localized;
            }

            return fallback ?? string.Empty;
        }

        public override string ToString() => Get(null);
    }
}