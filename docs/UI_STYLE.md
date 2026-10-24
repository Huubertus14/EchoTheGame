# Echo — UI-huisstijl

De goedgekeurde richting is het top-down hoofdmenu uit de ontwerpsessie: een donkere oceaan, een eenvoudige onderzeeër en cyaan sonarpulsen die de zeebodem als lichtpuntjes onthullen.

## Kleuren en typografie

| Rol | Kleur |
| --- | --- |
| Diepe achtergrond | `#020C13` |
| Vensters | `#051821` |
| Primaire actie, sonar, focus | `#9BE9E2` |
| Secundaire tekst | `#93B9C2` |
| Valuta en zeldzame contacten | `#F4B94F` |

Gebruik de gedeelde `EchoMenuTheme.asset` als bron. Liberation Sans SDF is de aanwezige, herbruikbare TMP-font. Dunne kapitalen met ruime letterafstand; grote titel, compacte navigatie, kleine instrumentlabels. Donkere tekst op de gevulde cyaan primaire knop. Overige knoppen zijn rustig en krijgen een duidelijke focus/hoverkleur.

## Compositie en gedrag

- Referentie: 1920 × 1080, schaalbaar Canvas met behoud van verhoudingen.
- Links: ECHO met een losse sonar-O, ondertitel en echte knoppen Spelen, Onderzeeër, Logboek, Instellingen, Afsluiten.
- Rechtsboven: losse onderzeeërbadge, level, XP-balk, losse valutamunt en saldo. Rechtsonder: losse signaalicoon en SONAR ACTIEF.
- Geen beste score in deze versie.
- Hoofdknop gevuld cyaan; overige opties transparant. Keyboard/controllerfocus zichtbaar, Escape sluit een venster.
- Vensters gebruiken dezelfde kleuren, ruime marges en echte sluit-/terugknoppen.
- Onderzeeër: eenvoudig silhouet van boven, weinig mechanisch detail.

## Assets en toekomstige live-achtergrond

`Assets/UI/Echo/Textures/MenuOcean.png` is uitsluitend achtergrondkunst, zonder tekst, knoppen of iconen. Gemaakt met de ingebouwde imagegen-tool, op basis van het goedgekeurde ontwerp. Prompt: behoud de top-down oceaan met eenvoudige onderzeeër en cyaan sonar; verwijder alle UI; houd links ruimte donker voor losse UI.

`Assets/UI/Echo/Icons/` bevat afzonderlijke transparante PNG's voor sonar, onderzeeërbadge, munt, pijl en signaal. De ronde knopvorm is eveneens een los sprite. De scene heeft een aparte `Backdrop / Replace with live ocean`-laag en een `Interface`-laag. Vervang later alleen de achtergrondafbeelding door een wereldcamera of RenderTexture met een rondvarende onderzeeër; laat Canvas, navigatie, iconen en controller behouden.

De scene is bewerkbaar zonder runtime-opbouw: alle UI-objecten en persistente button-events staan opgeslagen in MainMenu.unity. `Echo/Build Main Menu` bouwt de scene opnieuw vanuit de editorbron; voer dit alleen uit als je bestaande handmatige scene-aanpassingen wilt vervangen.

## Huidige functionaliteit

Spelen opent de bestaande Host/Join-routes. Onderzeeër en Logboek openen informatieve startvensters: upgrades/ontdekkingen zijn nog geen geïmplementeerde spelsystemen. Instellingen regelt de opgeslagen taalkeuze en volledig scherm. Een apart geluidsvenster bevat een aan/uit-toggle en sliders voor mastervolume, achtergrondmuziek en effecten. Teksten komen uit de Unity Localization-collection UI Text (Nederlands en Engels). Afsluiten heeft een bevestigingsvenster. Level 01, 0 valuta en lege XP zijn beginwaarden; `MenuController.SetProgress` is het aansluitpunt voor echte voortgang. De normale route blijft Bootstrap; bij rechtstreeks starten vanuit MainMenu maakt de controller de bestaande persistente managers aan als die nog ontbreken.

## Healthbar tijdens het spelen

`Assets/Prefabs/Players/PlayerHealthbar.prefab` is een bewerkbare world-space Canvas met een smalle, gesegmenteerde hull-meter. De navy ondergrond, seafoam vulling, dunne instrumentlijnen en amber waarschuwing bij maximaal 25% gezondheid komen uit `EchoMenuTheme.asset`. De meter volgt de speler en neemt geen muis-, toetsenbord- of controllerfocus over. `Echo / Style Player Health Bar` past deze opgeslagen prefabstijl opnieuw toe en vervangt de UI-kindobjecten; handmatige wijzigingen daaraan worden overschreven.

## Scorebord in Game

`Game.unity` bevat een bewerkbaar `Scoreboard`-Canvas met het gedeelde theme en echte TMP-velden. Het minimale paneel toont alleen de titel `Score`, gamemode/tijd en de tabel; geen gamenaam, icoon, telemetrielabels of instructieregels. Houd Tab of de View/Select-knop op de controller ingedrukt om het paneel te tonen; loslaten of focusverlies verbergt het direct. Het paneel neemt geen cursor-, gameplay- of navigatiefocus over. Bij veel spelers scroll je met muiswiel/pijltjes of de rechterstick.

De kolommen zijn rang, naam, score, kills, deaths, K/D en ping. Sortering: score aflopend, kills aflopend, deaths oplopend en vervolgens speler-ID. Je eigen rij heeft een cyaan markering; de eerste rang en ping vanaf 150 ms zijn amber. K/D gebruikt minimaal één death als deler. Ping komt van de host, wordt ongeveer elke seconde gesynchroniseerd en toont `--` zolang er geen meting is. De host zelf heeft 0 ms. De naamkolom heet `Naam`. De gamemode, matchstatus en resterende tijd blijven boven de tabel staan.

De lijst leest gerepliceerde spelerdata op objectidentiteit, zodat gelijke namen aparte rijen houden en laat aangesloten spelers bestaande scores ontvangen. De oude GameInfo-Canvas en automatische eindpopup zijn uit de game scene verwijderd; resultaten blijven na afloop via Tab beschikbaar. Escape behoudt de bestaande terugkeer naar het hoofdmenu. De aparte MainMenu-scene blijft de start- en verbindingsroute.

`Echo / Build Game Scoreboard` bouwt de opgeslagen game-UI opnieuw en maakt voorbeeldrenders op 16:9 en 5:4 in `docs/`. Dit vervangt de bestaande screen-space Canvas-objecten in Game; handmatige aanpassingen daaraan worden overschreven. De huidige scene is direct bruikbaar en hoeft niet opnieuw opgebouwd te worden.
