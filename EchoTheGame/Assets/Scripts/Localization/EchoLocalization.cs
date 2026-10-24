using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Project.Echo.Localization
{
    /// <summary>Unity Localization tables, persisted language choice and system-language fallback.</summary>
    public static class EchoLocalization
    {
        public const string TableName = "UI Text";
        private const string PreferenceKey = "Echo.Language";
        private static Task initialization;
        public static event Action Changed;
        public static bool IsReady { get; private set; }
        public static string SelectedCode => PlayerPrefs.GetString(PreferenceKey, "");
        public static IReadOnlyList<Locale> AvailableLocales => LocalizationSettings.AvailableLocales.Locales;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
            initialization = null;
            IsReady = false;
            Changed = null;
        }

        public static Task InitializeAsync() => initialization ??= InitializeInternalAsync();

        private static async Task InitializeInternalAsync()
        {
            await LocalizationSettings.InitializationOperation.Task;
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
            ApplyPreference();
            IsReady = true;
            Changed?.Invoke();
        }

        public static void Select(string code)
        {
            code ??= "";
            if (code.Length > 0 && LocalizationSettings.AvailableLocales.GetLocale(code) == null)
                return;
            PlayerPrefs.SetString(PreferenceKey, code);
            PlayerPrefs.Save();
            ApplyPreference();
            Changed?.Invoke();
        }

        private static void ApplyPreference()
        {
            var locales = LocalizationSettings.AvailableLocales;
            Locale locale = string.IsNullOrEmpty(SelectedCode)
                ? locales.GetLocale(Application.systemLanguage)
                : locales.GetLocale(SelectedCode);
            locale ??= locales.GetLocale("en");
            if (locale != null)
                LocalizationSettings.SelectedLocale = locale;
        }

        private static void OnLocaleChanged(Locale locale) => Changed?.Invoke();

        public static string Get(string key, params object[] arguments)
        {
            if (!IsReady) return key;
            string value = LocalizationSettings.StringDatabase.GetLocalizedString(TableName, key);
            return arguments == null || arguments.Length == 0 ? value :
                string.Format(LocalizationSettings.SelectedLocale.Identifier.CultureInfo ?? CultureInfo.InvariantCulture, value, arguments);
        }
    }
}
