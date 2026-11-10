using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameDev.Foundation;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using UnityEngine;

namespace GameDev.UnityServices
{
    // Authentication can be injected to link Steam/Apple/Google tickets instead of creating
    // an anonymous identity. The caller owns the native SDK and secure ticket acquisition.
    public sealed class UnityServicesAdapter : IIdentityAdapter, ICloudSaveAdapter
    {
        private readonly Func<Task> signIn;
        private readonly string authenticationProfile;
        public UnityServicesAdapter(Func<Task> signIn = null, string authenticationProfile = "default")
        { this.signIn = signIn; this.authenticationProfile = authenticationProfile; }
        public string Name => "Unity";
        public int Priority => 10;
        public PlatformFeatures Features => PlatformFeatures.Identity | PlatformFeatures.CloudSave;
        public bool IsAvailable => !string.IsNullOrEmpty(Application.cloudProjectId);
        public string AccountId => AuthenticationService.Instance.PlayerId;
        public async Task InitializeAsync()
        {
            if (Unity.Services.Core.UnityServices.State != ServicesInitializationState.Initialized) await Unity.Services.Core.UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                AuthenticationService.Instance.SwitchProfile(authenticationProfile);
                if (signIn != null) await signIn();
                else await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
        }
        public async Task<PlatformIdentity> GetIdentityAsync() => new(Name, AccountId,
            await AuthenticationService.Instance.GetPlayerNameAsync());
        public async Task<CloudDocument> LoadAsync(string key)
        {
            var result = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { key });
            return result.TryGetValue(key, out var item) ? new CloudDocument(item.Value.GetAs<string>(), item.WriteLock) : default;
        }
        public async Task<string> SaveAsync(string key, string json, string writeLock)
        {
            var result = await CloudSaveService.Instance.Data.Player.SaveAsync(new Dictionary<string, SaveItem>
            { { key, new SaveItem(json, writeLock) } });
            return result[key];
        }
    }
}
