# Echo: lobby, menu's en voortgang

## Gebruiken

Start zoals gebruikelijk via Bootstrap of MainMenu. **Spelen** vraagt een opgeslagen spelersnaam. **Kamer maken** genereert een kamercode van zes tekens; een tweede speler vult die code in en kiest **Aansluiten**. De kamer heeft maximaal acht spelers. Iedereen zet zichzelf op **Gereed** en alleen de host start de ronde. De aftelling duurt vijf seconden. Tijdens een ronde is de kamer gesloten; bij opnieuw spelen gaat hij weer open.

Een ongeldige code blijft in het menu met een foutmelding. Een volle, gesloten of ontbrekende kamer en verbindingsverlies keren terug naar het verbindingsscherm met uitleg. Er is een verbindingstimeout van dertig seconden. Escape annuleert het wachten op een verbinding.

**Escape / controller Start** opent het pauzemenu. De online simulatie loopt door, maar lokale beweging, schieten en sonar krijgen nulinput. Het menu biedt verder spelen, instellingen, besturing, profiel en verlaten met bevestiging. Het scorebord blijft via Tab/View beschikbaar wanneer geen interactief venster openstaat.

**Instellingen → Meer instellingen** bevat schermmodus, resolutie, beeldkwaliteit, FPS-limiet, VSync, taal en de bestaande drie volumekanalen. Geluid krijgt geen nieuwe content. Volumewijzigingen gelden meteen; beeld/taal via **Toepassen**. Schermmodus en resolutie zijn alleen op desktop instelbaar. VSync kan de ingestelde FPS-limiet op desktop overrulen.

**Besturing** toont de vaste controllerindeling en biedt opgeslagen toetsenbindingen voor vooruit, achteruit, links, rechts, boost, schieten en sonar. Dubbele bindingen en gereserveerde Escape/Tab worden geweigerd. Escape annuleert het kiezen van een toets. De controller gebruikt de bestaande legacy Input Manager met aparte joystickassen, zodat oude WASD-assen aangepaste toetsen niet alsnog overrulen. Dit is geen touchbesturing en nog geen volledige remapping van alle controllerindelingen.

Aan het einde verschijnt een apart resultaatvenster met de eindstand, score, kills/deaths en verdiende XP. De host kan **Opnieuw spelen** kiezen; iedereen keert terug naar dezelfde lobby en zet zich opnieuw gereed. Scores, gezondheid, respawnstatus en nog actieve projectielen worden gereset. Voortijdig verlaten geeft geen voltooiings-XP.

**Profiel / Logboek** toont naam, level, totale XP en carrièretotalen. Naamwijzigingen gelden bij de volgende verbinding. Het hoofdmenu leest echte XP/levelvoortgang. Er zijn nog geen valuta, levelbeloningen of unlocks aangesloten.

## XP voor later balanceren

`EchoTheGame/Assets/Settings/Resources/Echo/EchoProgression.asset` bevat de voorlopige regels:

- 100 XP voor een uitgespeelde ronde;
- 50 extra XP voor een overwinning tegen een andere speler;
- 10 XP per kill;
- 1000 XP per level.

Dit zijn ontwikkelwaarden. De regels zitten in een ScriptableObject; wijzigen vereist geen aanpassing van de opslag- of UI-code. Elke ronde krijgt een unieke ID. Een resultaat kan daardoor niet nogmaals XP opleveren door hetzelfde scherm opnieuw te tonen. De laatste twintig resultaten en alle reeds verwerkte ronde-ID's worden opgeslagen.

## Package en opslag

Zie [de zelfstandige Game Dev-packages](../LocalPackages/README.md) voor installatie, API's, Unity-services, Steam en toekomstige platformadapters. Echo gebruikt drie lokale UPM-packages buiten zijn Assets-map. Steamworks.NET is een gepinde Git-dependency. De eigen packagecode is direct lokaal bewerkbaar en heeft geen Echo-/Fusion-afhankelijkheden.

Profielen staan onder `Application.persistentDataPath/Foundation/player` in een build. Editorinstanties krijgen een map met hun projectnaam, zodat ParrelSync-klonen eigen profielen gebruiken. Het primaire bestand is `profile.json`, met `.bak` als vorige leesbare versie. Instellingen gebruiken de productgebonden Unity PlayerPrefs. Verwijder deze bestanden niet als je voortgang wilt behouden.

Unity Authentication en Cloud Save zijn echte SDK-adapters. Echo is nog niet gekoppeld aan een Unity-cloudproject; cloudfunctionaliteit blijft daarom uit. Een lokaal profiel is direct beschikbaar; wachten op online initialisatie blokkeert het menu niet. Handmatige cloudsynchronisatie staat in het profielvenster, inclusief een expliciete lokale/cloudkeuze bij conflicten.

`GameDevSteamSettings.asset` wordt geleverd met Steam uit en App ID 0. Na je eigen Steamworks-configuratie kan de adapter Steam ID, gebruikersnaam en native achievements verzorgen. Het toevoegen van automatische Steam-ticketuitwisseling met Unity is geblokkeerd door automatische goedkeuringscontrole en is niet geïmplementeerd. De huidige Unity-anonieme login is geen platformoverschrijdende accountkoppeling.

Switch en native mobiele platformdiensten hebben dezelfde aansluitinterfaces, maar hun SDK-adapters, accounts en certificering zijn nog toekomstwerk. De package claimt geen native mogelijkheden die niet zijn ingesteld.

## Onderhoud en validatie

**Echo → Install Game Foundation** werkt de eigen shell, verbindingsvelden en profielkoppelingen bij in MainMenu en Game. Bestaande arena, scorebord, HUD en achtergrond blijven staan. De UI is opgeslagen als echte bewerkbare scene-objecten en gebruikt `EchoMenuTheme.asset`. De bestaande menu-/scorebordbuilders nemen de shell mee. Zoals bij de andere builders worden handmatige wijzigingen binnen de gegenereerde shell bij opnieuw installeren vervangen.

Geautomatiseerde controles staan in `EchoFoundationVerification` en `EchoFoundationPlayModeCheck`. De eerste controleert opslag/back-up, ronde-ID's, XP, providerfallback, cloudconflicten/write locks/accountisolatie en achievement-retries. De tweede gebruikt uitsluitend een lokale Fusion Single-ronde, zonder verbinding met Photon of platformaccounts. De verificatierapporten in deze map vermelden de uitgevoerde controles. UI-previews zijn beschikbaar voor 16:9 en 5:4.

Twee echte netwerkclients, latency/uitval/hostmigratie, Steam met een eigen App ID, live Unity-cloudopslag en console-/mobiele apparaten moeten nog afzonderlijk functioneel worden getest. Lokale carrièretotalen zijn geen beveiligde ranked backend.
