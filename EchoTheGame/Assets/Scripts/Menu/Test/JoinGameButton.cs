using System;
using Project.Echo.Setting;
using Project.Echo.Setting.Session;
using TMPro;
using UnityEngine;

namespace Project.Echo.Menu.Test
{
    // Compatibility for older authored menus.
    public class JoinGameButton : BaseGameButton
    {
        [SerializeField] private TMP_InputField roomCode;
        protected override async void OnClick()
        {
            if (!roomCode) { Debug.LogWarning("Assign a room-code field or upgrade the menu with Echo/Install Game Foundation."); return; }
            try
            {
                await EchoFoundation.InitializeAsync();
                await SessionLauncher.LaunchAsync(false, roomCode.text, EchoFoundation.Services.Store.Profile.displayName);
            }
            catch (Exception exception) { Debug.LogException(exception); }
        }
    }
}
