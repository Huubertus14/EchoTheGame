# Jager — eerste speelbare klasse

Iedere speler vaart in deze proefversie met de Jager. Er is nog geen klassekeuze. De lobby toont de klasse en het maximale aantal levenspunten.

| Actie | Toetsenbord | Xbox-indeling |
| --- | --- | --- |
| Primaire aanval: doelzoekende torpedo | Spatie | A |
| Tweede actie: sonarscan | Linker Ctrl | X |

De toetsen zijn aanpasbaar onder **Instellingen → Besturing**. De sonarscan is een voorlopige tweede actie en doet geen schade.

Pas de balans buiten Play Mode aan in de Inspector van de assets onder `Assets/Settings`.

| Asset | Instellingen |
| --- | --- |
| `Submarines/Jager.asset` | Levenspunten, respawn, vaarsnelheid, draaien, boost en schietinterval |
| `Sonar/JagerSonar.asset` | Pinginterval (**Cooldown**), detectietijd en de instellingen van de actieve puls en botsingspuls |
| `Weapons/JagerTorpedo.asset` | Snelheid, schade, bereik, levensduur, doelzoekhoek en draaisnelheid |
| `Resources/Echo/ImpactSonar.asset` | Sonarpuls bij impact en levensduur van de emitter |
| `Resources/Echo/FreeForAll.asset` | Minimum en maximum spelers, aftellen, tijd-, kill- en scorelimiet en punten per kill |

Het spelersmaximum is instelbaar van 1 tot 8, passend bij de lobby en het resultaatvenster. Maak na een wijziging van het maximum een nieuwe kamer aan. Een tijd-, kill- of scorelimiet op 0 staat uit. Bij meerdere actieve limieten eindigt de ronde zodra de eerste wordt bereikt.

- **Romp:** 65 HP bij spawn en respawn, tegenover de vorige standaard van 100 HP.
- **Torpedo:** 25 Unity-eenheden per seconde, 28 schade en 0,8 seconde tussen schoten. De torpedo kiest bij vertrek het dichtstbijzijnde levende doel binnen 24 eenheden en 35° links of rechts van de schietrichting. Obstakels blokkeren de lock. Zonder lock vliegt hij rechtdoor; hij verliest de lock als het doel achter dekking verdwijnt.
- **Scan:** onthult onderzeeërs binnen 18 eenheden voor 3 seconden, ook als ze achter een obstakel liggen. Alleen de Jager die scant krijgt de contacten te zien. De sonargolf zelf is wel zichtbaar voor anderen. De eerste puls kan tot 12 reflecties geven, met maximaal 4 weerkaatsingen per pad. Elke weerkaatsing behoudt 80% van de helderheid. De scan is elke 4 seconden beschikbaar.

**Proef met twee spelers:** vaar achter een rots en controleer dat de andere speler je daar pas na een scan ziet. Schiet vervolgens in open water net naast die speler om te voelen hoeveel de torpedo bijstuurt. Test ook een schot buiten de richtkegel en een doel dat achter dekking wegvaart.

De Bastion en Spook zijn nog niet speelbaar. Voor de Bastion is het ontwerpdoel een overwinning in een frontaal duel bij gelijke omstandigheden; die balans kan pas worden getest als de Bastion bestaat.
