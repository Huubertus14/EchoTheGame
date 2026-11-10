using System;
using Project.Echo.Setting;
using Project.Echo.Setting.Session;
using UnityEngine;

namespace Project.Echo.Menu.Test
{
    // Compatibility for older authored menus; the current menu uses EchoGameShell.
    public class HostGameButton : BaseGameButton
    {
        protected override async void OnClick()
        {
            try
            {
                await EchoFoundation.InitializeAsync();
                await SessionLauncher.LaunchAsync(true, null, EchoFoundation.Services.Store.Profile.displayName);
            }
            catch (Exception exception) { Debug.LogException(exception); }
        }
    }
}
