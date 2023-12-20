using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.Localization;

namespace PlayableKids
{
    internal static class LocalizationManager
    {
        private static readonly Dictionary<string, TextObject> localization = new Dictionary<string, TextObject>();

        public static TextObject GetLocalizationKey(this string key)
        {
            if (!localization.TryGetValue(key, out var loc))
            {
                loc = new TextObject(key);
                localization[key] = loc;
            }
            return loc;
        }

        public static string Localized(this string key)
        {
            return key.GetLocalizationKey().ToString();
        }

        public static string Localized(this string key, Dictionary<string, object> attributes)
        {
            var newLoc = key.GetLocalizationKey().CopyTextObject();
            foreach (var attr in attributes)
                newLoc.Attributes.Add(attr.Key, attr.Value);
            return newLoc.ToString();
        }
    }
}
