# EchoTheGame

## Lokalisatie, geluid en ontwikkelstatus

Het menu heeft Nederlandse en Engelse vertaaltabellen via Unity Localization, een opgeslagen taalkeuze en een apart geluidsvenster met mute, mastervolume, achtergrondmuziek en effecten. Nieuwe talen uit Unity's Available Locales verschijnen automatisch in de keuzelijst. Koppel toekomstige muziek/effecten via het component `AudioChannel`.

Zie [ontwikkelstatus en vervolgstappen](docs/DEVELOPMENT_NEXT_STEPS.md) voor de uitgevoerde code-opruiming en controles. **Fusion 2 is nog niet geïmporteerd:** de officiële SDK-download vereist een Photon-login. Het opgegeven nieuwe AppId staat in dat document, klaar voor de migratie. De huidige checkout behoudt de werkende Fusion 1-configuratie.

Herstelpunt: `backup/pre-fusion2-20261024`. Werkbranch: `codex/fusion2-localization-audio`. Compilatie en editor-/layoutcontrole zijn uitgevoerd; functionele speeltests doet de eigenaar.

## Starten in Unity

De huidige herstelversie is getest in Unity **6000.6.7f1**. Het project is oorspronkelijk gemaakt met Unity 2021.3.11f1 en gebruikt Photon **Fusion 1**.

1. Open de projectmap `EchoTheGame` in Unity Hub.
2. Wacht tot packages, imports en scriptcompilatie klaar zijn.
3. Open `Assets/Scenes/Bootstrap.unity` en druk op Play. Deze scene maakt de persistente managers aan en opent het hoofdmenu. Rechtstreeks starten vanuit `MainMenu.unity` wordt ook ondersteund; ontbrekende managers worden dan aangemaakt. Start niet rechtstreeks vanuit de Game-scene.
4. Kies **Spelen → Duik starten** (Host). De speler verschijnt en na vijf seconden begint een testronde van dertig seconden.
5. Voor een tweede speler: start een tweede instantie (bijvoorbeeld via de meegeleverde ParrelSync-tool) en kies **Spelen → Aansluiten** (Join). Beide gebruiken momenteel de vaste sessienaam `TestGame` en dezelfde Photon-configuratie. Deze verbinding met een tweede speler moet nog worden getest.

Editorbesturing: W/S bewegen, A/D draaien, linker Shift versnellen, spatie schieten en linker Ctrl sonar. De huidige inputcode gebruikt een toetsenbord; mobiele touchbesturing en Android/iOS-builds zijn in deze herstelronde niet getest of toegevoegd.

## Hoofdmenu en huisstijl

`MainMenu.unity` bevat het top-down sonarontwerp met echte Unity-knoppen, TMP-tekst en afzonderlijke PNG-iconen. De achtergrondafbeelding staat apart voor een toekomstige live-wereld. Beste score is weggelaten. Level 01, 0 valuta en lege XP zijn beginwaarden; er is nog geen voortgangseconomie aangesloten.

Spelen behoudt Host/Join. Onderzeeër en Logboek hebben eigen startvensters; hun spelsystemen volgen later. Instellingen biedt mastervolume en volledig scherm. Pijltjestoetsen navigeren, Enter activeert en Escape sluit een venster.

De herbruikbare huisstijl staat in [docs/UI_STYLE.md](docs/UI_STYLE.md) en `Assets/UI/Echo/EchoMenuTheme.asset`. [Preview van de Unity-scene](docs/main-menu-preview.png). De scene-objecten zijn direct bewerkbaar; het editorcommando **Echo → Build Main Menu** bouwt het menu opnieuw en vervangt handmatige scene-aanpassingen. **Echo → Validate Main Menu** controleert de aansluitingen en maakt previews voor 16:9 en 5:4.

Gecontroleerd in Unity: scriptcompilatie, 14 knoppen met aansluitingen, de vijf menuvensters, voortgangsweergave en renders op twee schermverhoudingen. In Play Mode zijn toetsenbordnavigatie, Spelen, Escape en de volumeslider gecontroleerd. De bestaande netwerkroute is gekoppeld; een nieuwe multiplayerverbinding is bij deze UI-wijziging niet getest.

## Herstel van de oude dependencies

- De ongebruikte Newtonsoft Unity-converters zijn verwijderd. Ze veroorzaakten compileerfouten doordat de Newtonsoft-basisbibliotheek ontbrak.
- UniTask is verwijderd: alleen het laadicoontje gebruikte daadwerkelijk een UniTask-aanroep. Dit draait nu via `Update`, met dezelfde rotatiesnelheid en automatische pauze wanneer het object uitgeschakeld is.
- Oude object-ID-aanroepen in Photon-editorcode zijn vervangen door objectreferenties of objecthashes. De optionele Fusion 1-hierarchyoverlay is uitgeschakeld vanaf Unity 6.4, omdat de oude scene-ID-callback daar is verwijderd.
- De Photon-inspectorbinding voor property drawers gebruikt op Unity 6 de bijgewerkte interne methodesignature.
- Een ongeldig `SerializeField`-attribuut op de networked projectile-count-property is verwijderd.
- TextMesh Pro Essential Resources zijn via de ingebouwde importer bijgewerkt voor de geinstalleerde Unity-versie.

De bestaande Fusion-runtime, netwerkproperties, voorspelling, lag compensation en physics/gameplaylogica zijn behouden. De wijzigingen aan Unity-versie, rendering, materialen, packages en de Fusion-runtime-DLL die al aanwezig waren na het openen/upgraden zijn niet teruggedraaid.

## Validatie en beperkingen

Gecontroleerd in de geopende Unity-editor: scriptcompilatie, Bootstrap naar MainMenu, Photon-host starten, speler spawnen, healthbar, scoreboard en wedstrijdtimer. De Photon-inspector initialiseerde na de compatibiliteitsaanpassing zonder de eerdere reflection-exception.

Nog apart valideren: een tweede client, synchronisatie en physics onder latency, host migration en een build op een fysiek mobiel apparaat. Er blijven deprecationwaarschuwingen uit oude SDK-code en een waarschuwing over de oude WebSocket-pluginimporter over. De herstelpatch maakt deze editorroute bruikbaar; hij vervangt geen SDK-upgrade of volledige platformtest.

Photon is in deze ronde niet gemigreerd. Een overstap naar een andere dienst raakt de Fusion-netwerkbehaviours, RPCs, voorspelling, projectielen, lag compensation en host migration en hoort daarom in een aparte wijziging.
