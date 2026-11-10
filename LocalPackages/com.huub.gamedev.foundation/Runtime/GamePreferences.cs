using System;
using UnityEngine;

namespace GameDev.Foundation
{
    public static class GamePreferences
    {
        public static readonly string[] Actions = { "Forward", "Reverse", "Left", "Right", "Boost", "Fire", "Sonar" };
        private static readonly KeyCode[] Defaults = { KeyCode.W, KeyCode.S, KeyCode.A, KeyCode.D, KeyCode.LeftShift, KeyCode.Space, KeyCode.LeftControl };
        public static bool InputBlocked { get; set; }
        public static bool MenuOpen { get; set; }
        public static int MenuInputFrame { get; set; } = -1;
        public static KeyCode Key(int action) => (KeyCode)PlayerPrefs.GetInt("GameDev.Input." + Actions[action], (int)Defaults[action]);
        public static bool Held(int action) => !InputBlocked && Input.GetKey(Key(action));
        public static bool Bind(int action, KeyCode key)
        {
            if (key == KeyCode.Escape || key == KeyCode.Tab || key == KeyCode.None || (int)key >= (int)KeyCode.JoystickButton0) return false;
            for (int i = 0; i < Actions.Length; i++) if (i != action && Key(i) == key) return false;
            PlayerPrefs.SetInt("GameDev.Input." + Actions[action], (int)key); PlayerPrefs.Save(); return true;
        }
        public static void ResetBindings()
        {
            foreach (string action in Actions) PlayerPrefs.DeleteKey("GameDev.Input." + action);
            PlayerPrefs.Save();
        }
        public static bool Desktop => !Application.isMobilePlatform && Application.platform != RuntimePlatform.Switch;
        public static int FrameLimit => PlayerPrefs.GetInt("GameDev.Fps", 60);
        public static int Quality => Mathf.Clamp(PlayerPrefs.GetInt("GameDev.Quality", QualitySettings.GetQualityLevel()), 0, QualitySettings.names.Length - 1);
        public static bool VSync => PlayerPrefs.GetInt("GameDev.VSync", 1) == 1;
        public static int ScreenMode => PlayerPrefs.GetInt("GameDev.ScreenMode", Screen.fullScreenMode == FullScreenMode.Windowed ? 0 : 1);
        public static void SetGraphics(int quality, bool vsync, int fps, int mode, int width, int height)
        {
            PlayerPrefs.SetInt("GameDev.Quality", Mathf.Clamp(quality, 0, QualitySettings.names.Length - 1));
            PlayerPrefs.SetInt("GameDev.VSync", vsync ? 1 : 0);
            PlayerPrefs.SetInt("GameDev.Fps", fps);
            if (Desktop)
            {
                PlayerPrefs.SetInt("GameDev.ScreenMode", mode);
                PlayerPrefs.SetInt("GameDev.Width", width); PlayerPrefs.SetInt("GameDev.Height", height);
            }
            PlayerPrefs.Save(); Apply();
        }
        public static void Apply()
        {
            QualitySettings.SetQualityLevel(Quality, true);
            QualitySettings.vSyncCount = Desktop && VSync ? 1 : 0;
            Application.targetFrameRate = FrameLimit;
            if (Desktop && PlayerPrefs.HasKey("GameDev.Width"))
                Screen.SetResolution(Mathf.Max(640, PlayerPrefs.GetInt("GameDev.Width")), Mathf.Max(480, PlayerPrefs.GetInt("GameDev.Height")),
                    ScreenMode == 0 ? FullScreenMode.Windowed : FullScreenMode.FullScreenWindow);
        }
    }
}
