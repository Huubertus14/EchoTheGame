# Game Dev Foundation

Reusable local profiles, XP/levels, preferences, achievements and platform service contracts. See [package setup and platform support](../README.md).

Core usage:

```csharp
var platforms = new PlatformServices();
PlatformAdapterRegistry.Install(platforms);
var game = new Foundation(saveDirectory, "my_game_profile_v1", platforms);
// game.Store is available immediately, including without network access.
await game.InitializeAsync();
await game.SyncAsync();

game.Store.RecordMatch(new MatchRecord {
    id = uniqueRoundId, kills = kills, deaths = deaths, score = score, won = won
}, progressionRules);
```

Use a game-specific `saveDirectory` and cloud key. A round ID must be stable for all callbacks for the same completed round. Keep `schemaVersion` migration explicit. `Store.Profile` is a mutable DTO: call `Store.Save()` for manual mutations. File writes target the configured directory only, with a backup of the previous readable profile. Consumers should subscribe to `Store.Changed` for UI updates.

Register `IIdentityAdapter`, `ICloudSaveAdapter` and `IAchievementAdapter` implementations for additional platforms. `ICloudSaveAdapter.AccountId` must be the account that owns that cloud service, even if a different adapter supplies the displayed native identity. Keep SDK dependencies inside their own adapter assembly and platform constraints.
