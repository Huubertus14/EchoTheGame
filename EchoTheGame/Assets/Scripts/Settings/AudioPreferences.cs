using System;
using UnityEngine;

namespace Project.Echo.Setting
{
    /// <summary>Master gain is global; music/effects gain is applied by AudioChannel components.</summary>
    public static class AudioPreferences
    {
        private const string VolumeKey = "Echo.Audio.Volume";
        private const string MuteKey = "Echo.Audio.Muted";
        private const string MusicKey = "Echo.Audio.Music";
        private const string EffectsKey = "Echo.Audio.Effects";
        public static event Action Changed;
        public static float MasterVolume { get; private set; }
        public static float MusicVolume { get; private set; }
        public static float EffectsVolume { get; private set; }
        public static bool Muted { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            Changed = null;
            MasterVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(VolumeKey, 0.8f));
            MusicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicKey, 1f));
            EffectsVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(EffectsKey, 1f));
            Muted = PlayerPrefs.GetInt(MuteKey, 0) != 0;
            Apply();
        }

        public static void SetMasterVolume(float value)
        {
            MasterVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(VolumeKey, MasterVolume);
            Apply();
        }

        public static void SetMusicVolume(float value)
        {
            MusicVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(MusicKey, MusicVolume);
            Apply();
        }

        public static void SetEffectsVolume(float value)
        {
            EffectsVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(EffectsKey, EffectsVolume);
            Apply();
        }

        public static void SetMuted(bool value)
        {
            Muted = value;
            PlayerPrefs.SetInt(MuteKey, value ? 1 : 0);
            Apply();
            PlayerPrefs.Save();
        }

        private static void Apply()
        {
            AudioListener.volume = Muted ? 0f : MasterVolume;
            Changed?.Invoke();
        }
    }
}
