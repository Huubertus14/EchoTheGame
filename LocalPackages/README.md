# Game Dev packages

Game-independent Unity Package Manager packages. They live outside Echo's `Assets` and can be edited in place. Echo references them with `file:../../LocalPackages/...`; the code is not copied into `Library/PackageCache`. They have no Echo, Fusion, scene or UI-theme dependencies. The current repository tracks the packages; moving this folder to a separate repository later only requires updating UPM paths.

| Package | Responsibility |
| --- | --- |
| `com.huub.gamedev.foundation` | Local profiles, safe file persistence with backup, configurable XP/levels, duplicate-round protection, local achievements with a retry queue, preferences, platform capability selection and cloud conflict orchestration. |
| `com.huub.gamedev.unity-services` | Actual Unity Authentication and Cloud Save SDK adapter. Optional to the foundation. |
| `com.huub.gamedev.steam` | Actual Steamworks.NET adapter for Steam ID, persona name and native achievement storage/callbacks. Desktop only. Optional to the foundation. |

## Reuse in another Unity project

Add the foundation using **Package Manager → Add package from disk** and select its `package.json`. Add the Unity-services package if needed. For Steam also add the pinned SDK Git dependency `https://github.com/rlabrecque/Steamworks.NET.git?path=/com.rlabrecque.steamworks.net#2025.164.1` and the local Steam package. Keep the foundation as a direct local dependency so the dependent packages resolve it locally.

Create `PlatformServices`, register adapters, call `PlatformAdapterRegistry.Install(platforms)` for auto-registered native adapters, then construct `Foundation(saveDirectory, cloudKey, platforms)`. Initialize local UI before awaiting online services. Call `Store.RecordMatch` only for completed rounds using a unique ID. `ProgressionRules` is a ScriptableObject; game rules and unlocks are owned by the game. No economy, unlocks or achievement definitions are imposed by this package.

Adapters advertise real capabilities. Higher priority selects each service independently, so Steam identity/achievements can coexist with Unity Cloud Save. Initialization failures are recorded in `Platforms.Diagnostics`; local progress remains usable. A native service is not reported as available just because the build target is supported.

## Unity setup

Link a Unity cloud project under **Project Settings → Services**, enable Authentication and Cloud Save in that project's dashboard, and configure allowed identity providers. There are no project secrets in these packages. Echo currently has no cloud project ID; its adapter therefore remains unavailable and saves stay local.

The default adapter uses Unity anonymous Authentication and gets a Unity player name. Anonymous identities are device-local identities and do **not** automatically provide account recovery or cross-device progression. An injected `Func<Task>` sign-in delegate is the extension point for a properly configured linked sign-in flow. Native Steam persona names and Unity player names are separate identities. Automatic Steam-ticket forwarding to Unity has not been implemented; it requires explicit approval and provider configuration.

Cloud Sync uses SDK write locks, checks the authenticated cloud account, preserves local progress on errors and asks the game UI to resolve divergent progressed profiles. Use `SyncAsync(true)` to keep local progress, `SyncAsync(false)` to choose cloud progress. Neither button merges XP. The default Cloud Save access class is player-writable: this is an offline-friendly foundation, not a trusted ranked progression backend. Move authoritative rewards to Cloud Code/protected storage before relying on anti-cheat guarantees.

## Steam setup

Create **Game Dev → Steam Settings**, save as `Resources/GameDevSteamSettings.asset`, enter your own App ID and explicitly enable it. Echo includes a disabled asset with App ID 0. Use `enableInEditor` only when testing against your own app. Run Steam and configure the development `steam_appid.txt` using your App ID; do not distribute that development file. The adapter verifies the active App ID before exposing identity or achievements. It owns `SteamAPI.Init`, callbacks and shutdown; do not install another `SteamManager` alongside it.

Achievement IDs passed to `UnlockAchievementAsync` must be API names already defined and published in Steamworks. Failed submissions remain pending locally and retry when the native service is available. The current SDK synchronizes initial user stats through the Steam client; storing an unlock awaits the corresponding `UserStatsStored_t` callback. No achievements have been invented or published for Echo.

Steam Cloud file synchronization can later be configured in Steamworks for this game's save directory. The current cloud adapter intentionally uses Unity Cloud Save; Steam Cloud is not being simulated by a local-file provider.

## Future platforms

Android/iOS currently use the local foundation and, when configured, Unity Authentication/Cloud Save. Add Google Play Games / Game Center adapters for native names, credentials and achievements. There is no mobile touch UI in these packages.

Switch requires Nintendo developer access, licensed SDKs and platform-specific adapters/storage. No Nintendo binaries or fake Switch achievement implementation are included. Desktop display settings are disabled on mobile and Switch. Safe-area layout and device-specific input/certification still require actual device work.

## Sources

- [Unity local package paths](https://docs.unity3d.com/6000.0/Documentation/Manual/upm-localpath.html)
- [Unity player names](https://docs.unity.com/en-us/authentication/player-name-management)
- [Cloud Save SDK and write locks](https://docs.unity.com/en-us/cloud-save/tutorials/unity-sdk)
- [Steamworks.NET installation](https://steamworks.github.io/installation/)
- [Pinned Steamworks.NET release](https://github.com/rlabrecque/Steamworks.NET/releases/tag/2025.164.1)
