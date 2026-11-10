using System.IO;
using GameDev.UnityServices;
using Project.Echo.Setting;
using UnityEngine;

namespace Project.Echo.Networking
{
    public static class EchoPlatformBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            EchoFoundation.ConfigurePlatforms += services =>
            {
                string profile = Application.isEditor ? Path.GetFileName(Path.GetDirectoryName(Application.dataPath)).ToLowerInvariant() : "default";
                services.Register(new UnityServicesAdapter(authenticationProfile: profile));
            };
        }
    }
}
