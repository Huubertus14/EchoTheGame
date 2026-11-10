using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Fusion;
using Project.Echo.Loading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Echo.Setting.Session
{
    public static class SessionLauncher
    {
        private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        public static string NewCode()
        {
            var bytes = new byte[6];
            using (var random = RandomNumberGenerator.Create()) random.GetBytes(bytes);
            var code = new char[6];
            for (int i = 0; i < code.Length; i++) code[i] = Alphabet[bytes[i] % Alphabet.Length];
            return new string(code);
        }
        public static string NormalizeCode(string code) => (code ?? "").Trim().ToUpperInvariant();
        public static bool ValidCode(string code)
        {
            if (code == null || code.Length != 6) return false;
            foreach (char character in code) if (Alphabet.IndexOf(character) < 0) return false;
            return true;
        }
        public static async Task LaunchAsync(bool host, string code, string displayName)
        {
            if (EchoFoundation.IsConnecting) return;
            code = host ? NewCode() : NormalizeCode(code);
            if (code.Length > 0 && !ValidCode(code)) throw new ArgumentException("ROOM_CODE_INVALID");
            EchoFoundation.IsConnecting = true;
            try
            {
                await EchoFoundation.InitializeAsync();
                EchoFoundation.Services.Store.SetName(displayName);
                Settings.Player = new PlayerSettings { PlayerName = EchoFoundation.Services.Store.Profile.displayName };
                Settings.Session = new SessionSettings(code, host ? GameMode.Host : GameMode.Client);
                EchoFoundation.ConnectionMessage = null;
                LoadScreenController.Show();
                var load = SceneManager.LoadSceneAsync("Game");
                while (load != null && !load.isDone) await Task.Yield();
            }
            catch { EchoFoundation.IsConnecting = false; LoadScreenController.Hide(); throw; }
        }
    }
}
