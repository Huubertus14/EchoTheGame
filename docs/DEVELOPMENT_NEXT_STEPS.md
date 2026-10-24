# Echo: voortgang en vervolgwerk

## Uitgevoerd op 24 oktober 2026

- Herstelpunt: Git-tag `backup/pre-fusion2-20261024` op commit `92689da`.
- Werkbranch: `codex/fusion2-localization-audio`.
- Unity Localization 1.5.11, met dezelfde string-table-aanpak als Mexen.
- Hoofdmenu en alle menuvensters gekoppeld aan 27 vertaalsleutels, volledig in Nederlands en Engels.
- Taalinstellingen: systeemtaal, Nederlands en Engels. De keuzelijst volgt automatisch alle beschikbare Unity-locales; nieuwe vertaalde talen verschijnen zonder codewijziging. Een onbekende systeemtaal valt terug op Engels. De gekozen taal wordt opgeslagen.
- Een eigen geluidspagina met aan/uit-toggle en sliders voor mastervolume, achtergrondmuziek en effecten. Mute bewaart de ingestelde volumes. De bestaande mastervolumevoorkeur blijft behouden.
- Mastervolume geldt via `AudioListener.volume` in alle scenes. Plaats `AudioChannel` op een `AudioSource` en kies Music of Effects om die bron aan de betreffende slider te koppelen. Er is nog geen nieuwe muziek of effectcontent toegevoegd.
- Alle eigen scripts onder `Assets/Scripts` nagelopen; gerichte reparaties hieronder. Externe libraries zijn niet breed herschreven.

## Fusion 2: wacht op SDK

Het officiële SDK-bestand ontbreekt lokaal. De download vereist een ingelogd Photon-account. De checkout gebruikt daarom nog Fusion 1. De nieuwe AppId wordt pas actief wanneer SDK en scripts zijn gemigreerd; een Fusion 2-AppId in Fusion 1 zou de bestaande netwerkroute breken.

Nieuwe opgegeven AppId: `215465d8-ed02-41fa-9310-dcf5f9496338`.

Download [Fusion 2.0.13 Stable](https://downloads.photonengine.com/download/fusion/photon-fusion-2.0.13-stable-2379.unitypackage?pre=sp). Photon schrijft voor eerst naar de laatste 2.0.x te gaan voordat 2.1.x wordt geïmporteerd. De documentatie noemt Unity 6.0 en 6.3 expliciet; compatibiliteit met de huidige Unity 6.6 moet bij import en compilatie worden bevestigd. Zie de [SDK-voorwaarden](https://doc.photonengine.com/fusion/v2/getting-started/sdk-download).

Na ontvangst van de SDK, in deze volgorde:

1. Netwerkconfiguratie en prefab-/script-GUIDs veiligstellen; de Photon-map gecontroleerd vervangen door de officiële SDK.
2. Nieuwe AppId, scenes, prefabregistratie en runnerconfiguratie migreren.
3. `PlayerMovement` van `NetworkPositionRotation` naar de Fusion 2-transformaanpak omzetten met behoud van de huidige beweging.
4. Oude `OnChanged`/`Changed<T>`-callbacks vervangen. Gameplaybeslissingen over schade, sterven en matchstatus horen in de simulatie; visuele callbacks dienen de presentatie.
5. Projectile-bufferinterpolatie en callback-/scene-API aanpassen. Hit detection en spawn-/respawnregels behouden.
6. Host migration en herstel van spelerautoriteit en netwerkstaat migreren. Daarna compileren en prefab-/scenekoppelingen controleren. De eigenaar doet de speeltests.

## Code-opruiming

- Spelerslijst: geen dictionarymutatie tijdens enumeratie; hergebruikte HashSet voor inkomende namen en een verwijderlijst. Dubbele naamregistratie veroorzaakt geen exception meer.
- UI: wedstrijdtimer ververst weer elke halve seconde; de startmelding start slechts één fade-coroutine.
- Lifecycle: camera meldt zich af van het spawn-event; vernietigen van een dubbele Settings-instance wist de actieve singleton niet. Een dubbele loadingcontroller stopt direct zijn initialisatie.
- Sonar: gecachte materiaalinstance per object, die bij vernietiging wordt vrijgegeven.
- Objectpool: uitgestelde returns worden daadwerkelijk via Unity Update verwerkt.
- Beide terug-naar-menu-routes wachten op runner-shutdown; herhaalde verzoeken delen dezelfde operatie.
- Networking: startresultaten worden gecontroleerd; migratie krijgt input en observeert zijn async resultaat; de callbackhandler komt rechtstreeks van de eigen runner; de score wordt van de juiste scoreboardbehaviour gekopieerd.
- Guards bij ontbrekende lokale speler, lege killfeed, renderen vóór spawn, verdwenen migratiecontroller en een projectielstap zonder verplaatsing.
- Healthbar bewaart nu zijn verbindingstoken voor de bestaande cleanup-route.

Spelbalans, matchduur, killscores, bewegingssnelheid, schietinterval en netwerkstruct-layout zijn niet bewust gewijzigd. Er zijn geen speeltests uitgevoerd voor deze wijzigingen.

## Controles

Unity batchcompilatie met de huidige Fusion 1-SDK is geslaagd. Editorvalidatie controleert 16 knoppen, zes vensters, persistente button-events, instellingenbindings en alle 27 vertaalsleutels in beide talen. Menu, instellingen en geluid zijn gerenderd in NL/EN; het hoofdmenu ook op 16:9 en 5:4. Dit vervangt geen functionele speeltest.

Previews staan in `docs/main-menu-{nl,en}-preview.png`, `docs/main-menu-settings-{nl,en}-preview.png` en `docs/main-menu-audio-{nl,en}-preview.png`.

## Aanbevolen ontwikkelvolgorde

1. **Fusion-migratie afronden** zodra de SDK beschikbaar is; eventueel daarna gecontroleerd naar Fusion 2.1.
2. **Jouw functionele tests:** taal wisselen en herstarten, mute/volumes, keyboard/controller, twee clients, schieten/sonar, schade/respawn, matcheinde, opnieuw hosten en hostverlies.
3. **Mobiele basis:** touchbesturing, safe areas, schermverhoudingen, onderbreken/hervatten en wifi-/mobiele-datawissels op echte Android/iOS-apparaten.
4. **Vertraging en netwerkuitval:** jitter/pakketverlies simuleren en keuzes over host migration en herverbinden valideren.
5. **Gameflow en identiteit:** eigen kamercodes, foutmeldingen en retry, stabiele speler-ID in plaats van spelersnaam als sleutel en configureerbare wedstrijdregels.
6. **Audio en content:** muziek/effecten toevoegen en AudioChannel aansluiten; daarna echte onderzeeërvoortgang en logboekdata.
7. **Profileren op mobiel:** CPU/GPU, physics, netwerkverkeer, materiaalinstances en poolgebruik meten vóór grotere optimalisaties.

Bekende migratieaandachtspunten: de oude host-herstelroute roept `Spawned()` handmatig opnieuw aan; state-authority en presentatie zijn verweven. Deze onderdelen verdienen een gerichte Fusion 2-herziening in plaats van een cosmetische refactor.

## Verder vertalen

Open **Window > Asset Management > Localization Tables**, collection **UI Text**. Maak voor een nieuwe taal een Locale en een tabel met alle sleutels aan. De taal verschijnt automatisch in de instellingen. Koppel nieuwe TMP-labels met `EchoLocalizedTmpText`.

`Echo/Upgrade Localization and Settings` vult de standaardvertalingen opnieuw in; gebruik die niet om handmatig aangepaste vertalingen te bewaren. Tekstwijzigingen doe je rechtstreeks in de vertaaltabellen. `Echo/Build Main Menu` bouwt het hele menu opnieuw en vervangt handmatige scene-aanpassingen.
