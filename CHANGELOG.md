# Changelog Conspiratio.Lib

## 3.18.0

_18.07.2026_

**[DE]**
- Neue Klasse `PrivilegienManager` (mit `PrivilegInfo`) hinzugefügt: aktualisiert die Privilegien des aktiven Spielers, listet die derzeit gültigen (abhängig von Amt, Titel und Familienstand) und führt ein Privileg über dessen `PrivExecute` aus
- Behoben: Die Privilegien `PrivAmtNiederlegen` und `PrivJurist` blockierten den UI-Thread synchron auf einem asynchronen Ja/Nein-Dialog (`.GetAwaiter().GetResult()`) und wurden auf einen asynchronen Ablauf umgestellt, damit sie im Godot-Client nicht deadlocken

**[EN]**
- Added new class `PrivilegienManager` (with `PrivilegInfo`): refreshes the active player's privileges, lists the currently valid ones (depending on office, title and family status) and executes a privilege via its `PrivExecute`
- Fixed: the privileges `PrivAmtNiederlegen` and `PrivJurist` blocked the UI thread synchronously on an asynchronous yes/no dialog (`.GetAwaiter().GetResult()`) and were changed to an asynchronous flow so they no longer deadlock in the Godot client

## 3.17.0

_18.07.2026_

**[DE]**
- Spiellogik, die noch im Godot-Frontend lag, in die Lib gekapselt: `HandelsManager.HatPraesenzInStadt` (Wohnsitz oder Werkstätte mit Lager – für das Banner auf der Weltkarte), `HandelsManager.NaechsterProduktionsRohstoff` und `NaechsterVerkaufsRohstoff` (Weiterschalten auf den nächsten gültigen Rohstoff), `HandelsManager.SetzeVerkaufsStadtAusWert` (Zielstadt aus einem Zählerwert setzen, eigene Stadt überspringen, am Rand umbrechen) sowie `RundenManager.IstLetzterSpielerImJahr`

**[EN]**
- Encapsulated game logic that still lived in the Godot frontend into the Lib: `HandelsManager.HatPraesenzInStadt` (residence or workshop with storage – for the banner on the world map), `HandelsManager.NaechsterProduktionsRohstoff` and `NaechsterVerkaufsRohstoff` (advancing to the next valid resource), `HandelsManager.SetzeVerkaufsStadtAusWert` (setting the target town from a counter value, skipping the own town, wrapping at the edge) as well as `RundenManager.IstLetzterSpielerImJahr`

## 3.16.0

_18.07.2026_

**[DE]**
- Neue Klasse `KircheManager` hinzugefügt: die kirchlichen Tätigkeiten aus dem alten WinForms-Client – Kirchgang (Ablass kaufen, beichten, Waisenkind adoptieren), Konvertieren und Austreten sowie der Beitritt zu einer Konfession für Konfessionslose (Daten und Mutationen von der UI getrennt)

**[EN]**
- Added new class `KircheManager`: the church activities from the old WinForms client – the church visit (buy an indulgence, confess, adopt an orphan), converting and leaving the church as well as adopting a faith for the denominationless (data and mutations separated from the UI)

## 3.15.0

_18.07.2026_

**[DE]**
- Geändert: Beim Tod eines Spielers mit Erben wird das Amt des Verstorbenen nicht mehr an den Erben vererbt, sondern – wie bei jedem Todesfall – freigegeben und steht damit im nächsten Jahr zur Wahl (`FamilieManager.FuehreTestamentAus`); erbt der Ehepartner, wird auch dessen von der Erbübernahme übernommenes Amt freigegeben

**[EN]**
- Changed: when a player with an heir dies, the deceased's office is no longer inherited by the heir but – like in any death – freed and thus stands for election the next year (`FamilieManager.FuehreTestamentAus`); if the spouse inherits, their office taken over during the succession is freed as well

## 3.14.0

_18.07.2026_

**[DE]**
- Neue Klasse `FamilieManager` (mit `KupplerinVorschlag`, `WerbeGeschenk`, `GeschenkAuswahl`, `WerbungsErgebnis`, `HochzeitErgebnis`, `KindInfo`, `ErbeOption`, `TestamentErgebnis`) hinzugefügt: Partnersuche über die Kupplerin, jährliche Brautwerbung mit Geschenken (Verliebtheit steigt nach Gefallen und Bosheit), Hochzeit bei voller Verliebtheit (inkl. Titelangleich), Nachwuchs (Geburt mit Geschlecht und Namen, Kindestod) sowie das Testament (Erbe bestimmen unter Erzbistum/Ehepartner/Kindern und die Erbfolge beim Tod über `TestamentVollstrecken`) und `KIVerheiraten` am Jahresende
- Behoben: Der Kindestod nutzt nun den korrekten Kind-Slot-Bereich (im WinForms-Original lief die Schleife durch einen falschen Startindex nie, sodass Kinder faktisch nie starben)

**[EN]**
- Added new class `FamilieManager` (with `KupplerinVorschlag`, `WerbeGeschenk`, `GeschenkAuswahl`, `WerbungsErgebnis`, `HochzeitErgebnis`, `KindInfo`, `ErbeOption`, `TestamentErgebnis`): partner search via the matchmaker, yearly courtship with gifts (affection grows by liking and malice), marriage at full affection (including title adjustment), offspring (birth with gender and name, child death) as well as the will (choosing an heir among archbishopric/spouse/children and the succession on death via `TestamentVollstrecken`) and `KIVerheiraten` at the end of the year
- Fixed: child death now uses the correct child slot range (in the WinForms original the loop never ran due to a wrong start index, so children effectively never died)

## 3.13.0

_18.07.2026_

**[DE]**
- Neue Klasse `RundenEndeManager` hinzugefügt: die Todesfälle unter den KI-Spielern am Jahresende (feststehende Tode und Zufallstode nach der Sterbeformel) aus dem alten WinForms-Client; jeder Todesfall gibt über `KIstirbt` das Amt des Verstorbenen frei, wodurch neue Wahlen entstehen, und liefert eine Meldung "Name (Alter†)"

**[EN]**
- Added new class `RundenEndeManager`: the deaths among the AI players at the end of the year (fixed deaths and chance deaths by the death formula) from the old WinForms client; each death frees the deceased's office via `KIstirbt` (creating new elections) and returns a message "name (age†)"

## 3.12.0

_18.07.2026_

**[DE]**
- Neue Klasse `AemterManager` (mit `BewerbungsAngebot`, `BewerbungsErgebnis`, `WahlDetails`, `WahlKandidat`, `WahlWaehler`, `WahlAnsicht`, `WahlErgebnis`) hinzugefügt: Bewerbung um freie Ämter (An- und Abmelden zu einer Wahl, Infoanzeige der Wähler und Mitbewerber) sowie die Auszählung der Wahlen am Jahresende (Ermittlung der beteiligten Wahlen, Auflösung der Wähler über Stadt-, Land- und Reichsebene, KI-Stimmlogik nach Sympathie, Auswertung mit Los bei Stimmengleichheit, Amtsvergabe und Auffüllen der übrigen Ämter) aus dem alten WinForms-Client

**[EN]**
- Added new class `AemterManager` (with `BewerbungsAngebot`, `BewerbungsErgebnis`, `WahlDetails`, `WahlKandidat`, `WahlWaehler`, `WahlAnsicht`, `WahlErgebnis`): applying for free offices (registering and withdrawing from an election, info display of voters and competitors) as well as the resolution of elections at the end of the year (determining the involved elections, resolving voters across town, land and empire level, AI voting logic by sympathy, evaluation with a lot on a tie, office assignment and filling the remaining offices) from the old WinForms client

## 3.11.0

_18.07.2026_

**[DE]**
- Neue Klassen `AnwesenManager`, `HausAngebot` und `ErweiterungsAngebot` hinzugefügt: Wohnsitze bauen und umbauen (mit halbem Restwert als Fixpreisreduzierung), renovieren (Preis nach fehlendem Zustand, Abschluss bei Rundenende), erweitern (fehlende Hauserweiterungen mit Preis) und verkaufen (nur ohne verbleibende Werkstätten in der Stadt) aus dem alten WinForms-Client; alle Kosten berücksichtigen den Sparplan-Faktor (Privileg 15)

**[EN]**
- Added new classes `AnwesenManager`, `HausAngebot` and `ErweiterungsAngebot`: building and rebuilding residences (with half the remaining value as a fixed price reduction), renovating (price based on the missing condition, completed at the end of the turn), extending (missing house extensions with price) and selling (only without remaining workshops in the town) from the old WinForms client; all costs take the savings plan factor (privilege 15) into account

## 3.10.0

_17.07.2026_

**[DE]**
- Neue Klassen `SchreibstubeManager`, `KreditAngebot` und `KreditInfo` hinzugefügt: Kreditangebote des Geldleihers (zufälliger KI-Spieler, 10 % seines Vermögens, Zufallszins mit Privileg-30-Halbierung, 4 bis 7 Jahre Laufzeit), Kreditaufnahme (inkl. Deliktpunkt bei Kreditverbot und Statistik), Kreditbuch mit Tilgung sowie die Gesetzesanzeige (Ebenen-Überschrift mit Strenge-Bewertung und die zehn Gesetzestexte je Ebene) aus dem alten WinForms-Client

**[EN]**
- Added new classes `SchreibstubeManager`, `KreditAngebot` and `KreditInfo`: money lender credit offers (random AI player, 10% of his fortune, random interest halved with privilege 30, 4 to 7 years term), taking credits (including offense point when credits are forbidden and statistics), the credit book with repayment as well as the law display (level heading with strictness rating and the ten law texts per level) from the old WinForms client

## 3.9.1

_17.07.2026_

**[DE]**
- Spielstände werden jetzt als kompaktes JSON ohne Einrückung gespeichert (deutlich kleinere Dateien; zum Anschauen einfach in einem Editor mit JSON-Formatierung öffnen)

**[EN]**
- Savegames are now stored as compact JSON without indentation (much smaller files; to inspect them simply open them in an editor with JSON formatting)

## 3.9.0

_17.07.2026_

**[DE]**
- Spielstände werden jetzt als offene, von Hand editierbare JSON-Dateien (*.json) gespeichert — der BinaryFormatter des alten Formats wurde unter .NET 9+ entfernt und funktioniert im Godot-Client nicht mehr
- Die Serialisierung arbeitet feldbasiert wie zuvor der BinaryFormatter (alle Instanzfelder, Objekterzeugung ohne Konstruktor), dadurch war keine Änderung am Domänenmodell nötig; polymorphe Typen werden über ein abgesichertes $type-Feld aufgelöst, das nur Lib-Typen zulässt
- Alte *.dat-Spielstände werden beim Laden automatisch in das JSON-Format konvertiert, sofern die Runtime den BinaryFormatter noch unterstützt (z. B. im WinForms-Client); andernfalls erscheint ein Hinweis
- Neue Abhängigkeit: Newtonsoft.Json 13.0.4

**[EN]**
- Savegames are now stored as open, hand-editable JSON files (*.json) — the BinaryFormatter of the old format was removed in .NET 9+ and no longer works in the Godot client
- The serialization works field based like the BinaryFormatter did before (all instance fields, object creation without constructors), so no change to the domain model was necessary; polymorphic types are resolved via a secured $type field that only allows Lib types
- Old *.dat savegames are automatically converted to the JSON format when loading, provided the runtime still supports the BinaryFormatter (e.g. in the WinForms client); otherwise a hint is shown
- New dependency: Newtonsoft.Json 13.0.4

## 3.8.0

_17.07.2026_

**[DE]**
- Neue Klassen `SpeicherManager` und `SpielstandInfo` hinzugefügt: Speichern, Laden (inkl. Anreicherung alter Spielstände), Autosave mit Aufräumen des vorvorletzten Standes, Auflisten und Löschen von Spielständen — im selben *.dat-Format wie der WinForms-Client, Spielstände bleiben zwischen beiden Clients austauschbar
- Neue Klasse `SpielstandDeserializationBinder` (aus dem WinForms-Client übernommen): bildet Typnamen alter Spielstände auf die in die Lib ausgelagerten Typen ab

**[EN]**
- Added new classes `SpeicherManager` and `SpielstandInfo`: saving, loading (including enrichment of old savegames), autosave with cleanup of the second to last save, listing and deleting savegames — in the same *.dat format as the WinForms client, savegames stay exchangeable between both clients
- Added new class `SpielstandDeserializationBinder` (taken over from the WinForms client): maps type names of old savegames to the types moved into the Lib

## 3.7.0

_17.07.2026_

**[DE]**
- Neue Klassen `ZugNachrichtenManager` und `SchuldenProzessErgebnis` hinzugefügt: kapseln die Zugnachrichten-Ereignisse aus dem alten WinForms-Client — Gesetzesverstöße mit Strafen (Höchstzahl Anwesen, maximale Taler, Schlösserverbot), Statistik, Amtseinkommen, Anwesen-Aktualisierung (Bauzeit, Zustand, Renovierung), Sterbeprüfung mit Todesursachen-Text sowie den Schuldenprozess mit 11 Geschworenen und Schuldturm-Konsequenzen
- Neue Methode `DynamischeSpieldaten.EntferneAktivenSpielerAusDemSpiel`: entfernt den aktiven Spieler ohne Rückfrage aus dem Spiel (aus `AktivenSpielerEntfernen` extrahiert und dort wiederverwendet)
- Neue Methode `RundenManager.SchliesseZugAb`: Altern und Zug-Flags getrennt vom Spielerwechsel, damit die Zugnachrichten wie im Original mit dem neuen Alter rechnen

**[EN]**
- Added new classes `ZugNachrichtenManager` and `SchuldenProzessErgebnis`: encapsulate the turn message events from the old WinForms client — law violations with fines (maximum estates, maximum Taler, castle ban), statistics, office income, estate updates (construction time, condition, renovation), death check with cause of death text as well as the debt trial with 11 jurors and debtor's tower consequences
- Added new method `DynamischeSpieldaten.EntferneAktivenSpielerAusDemSpiel`: removes the active player from the game without confirmation (extracted from `AktivenSpielerEntfernen` and reused there)
- Added new method `RundenManager.SchliesseZugAb`: aging and turn flags separated from the player switch so the turn messages calculate with the new age like in the original

## 3.6.0

_17.07.2026_

**[DE]**
- Neue Klasse `HandelsManager` hinzugefügt: kapselt alle Stadt-Aktionen des aktiven Spielers aus der alten WinForms-Stadtansicht — Werkstätten kaufen/verkaufen, Rohstoffe direkt kaufen/verkaufen (inkl. Stadtvorrat-, Taler- und Lagerraum-Prüfung), Produktionsslots einstellen (Aktionsart, Produktions-/Verkaufsrohstoff, Arbeiter, Stätten, Verkaufsanzahl mit Lager-Reservierung, Zielstadt) sowie Karawanenwahl
- Neue Klassen `BuchManager` und `BuchErgebnis` hinzugefügt: wickeln zu Jahresbeginn die Aufträge des Vorjahres ab (Exporte mit Erlösgutschrift und Diebstahl, Produktion mit Qualität und Lagerverlust, Reservierung für permanente Verkäufe)

**[EN]**
- Added new class `HandelsManager`: encapsulates all town actions of the active player from the old WinForms town view — buying/selling workshops, buying/selling resources directly (including town stock, Taler and storage checks), configuring production slots (action type, production/sale resource, workers, sites, sale amount with storage reservation, target town) as well as caravan selection
- Added new classes `BuchManager` and `BuchErgebnis`: settle the previous year's orders at the start of the year (exports with revenue credit and theft, production with quality and storage loss, reservation for permanent sales)

## 3.5.0

_16.07.2026_

**[DE]**
- Neue Klassen `AbrechnungsManager` und `AbrechnungsErgebnis` hinzugefügt: berechnen und verbuchen die Jahresabrechnung des aktiven Spielers (Arbeiter-, Betriebs- und Transportkosten, Verkaufssteuern inkl. Steuerhinterziehungs-Privilegien, Informanten, Saboteure, Kreditzinsen, Kirchenzehnt, Zölle inkl. Auszahlung an die Zollburg-Besitzer und Zollfreiheits-Privilegien sowie Sold) aus dem alten WinForms-Dialog "Abrechnung"

**[EN]**
- Added new classes `AbrechnungsManager` and `AbrechnungsErgebnis`: calculate and book the yearly settlement of the active player (worker, operating and transport costs, sales taxes including tax evasion privileges, informants, saboteurs, loan interest, church tithe, customs including payout to the toll castle owners and toll exemption privileges as well as military pay) from the old WinForms dialog "Abrechnung"

## 3.4.0

_16.07.2026_

**[DE]**
- Neue Klasse `RundenManager` hinzugefügt: kapselt die Hot-Seat-Rundenrotation (Zugbeginn mit Privilegien-/Ansehensaktualisierung, Zugende mit Altern und Zug-Flags, Spielerwechsel, Jahreswechsel nach dem letzten Spieler, Schuldturm-Behandlung) aus dem alten WinForms-Client

**[EN]**
- Added new class `RundenManager`: encapsulates the hot seat turn rotation (turn start with privilege/reputation update, turn end with aging and turn flags, player switch, year change after the last player, debtor's tower handling) from the old WinForms client

## 3.3.0

_16.07.2026_

**[DE]**
- `PlayerSetupManager.ErstelleSpieler` erwartet nun das Flag `stadtGewaehlt`, damit eine vom Client vorab zufällig bestimmte Heimatstadt kostenlos bleibt
- Neue Methode `PlayerSetupManager.WuerfleZufaelligeStadt` zum Auswürfeln einer zufälligen Stadt-ID

**[EN]**
- `PlayerSetupManager.ErstelleSpieler` now expects the flag `stadtGewaehlt` so that a home town randomly pre-determined by the client stays free of charge
- Added new method `PlayerSetupManager.WuerfleZufaelligeStadt` for rolling a random town ID

## 3.2.0

_16.07.2026_

**[DE]**
- Neue Klasse `PlayerSetupManager` hinzugefügt: kapselt die Spielererstellung (Name-Validierung, Bannervergabe-Prüfung, transaktionales Erstellen mit Geschlecht, Banner, Religion, Heimatstadt und Rohstoff inkl. Zufalls- und Kostenlogik) aus dem alten WinForms-Dialog `SpielerHinzufuegen`
- Neue Klasse `PlayerSetupErgebnis` mit den aufgelösten Stadt- und Rohstoff-IDs einer Spielererstellung
- `NewGameManager.CreateNewGame` setzt die Spielwelt nun selbst über `NeuInitialisieren` zurück, bevor die Spieleinstellungen übernommen werden

**[EN]**
- Added new class `PlayerSetupManager`: encapsulates player creation (name validation, banner availability check, transactional creation with gender, banner, religion, home town and resource including random and cost logic) from the old WinForms dialog `SpielerHinzufuegen`
- Added new class `PlayerSetupErgebnis` holding the resolved town and resource IDs of a player creation
- `NewGameManager.CreateNewGame` now resets the game world itself via `NeuInitialisieren` before applying the game settings

## 3.1.0

_01.01.2026_

**[DE]**
- Kleinere Refaktorisierungen des `NewGameManagers`
- Refaktorisierung: Benenne `ITextAnzeigen` um in `IShowText`
- Füge Resource Datei für statische, übersetzbare Texte hinzu (bisher mit deutsch und englisch als Sprache)

**[EN]**
- Small refactorings of `NewGameManagers`
- Refactoring: Rename `ITextAnzeigen` to `IShowText`
- Added a resource file for static localizable text (until now with german and english as languages)

## 3.0.0

_23.12.2024_

**[DE]**
- Projekt auf .NET Standard 2.0 angehoben
- Neue Überladung für Methode "Initialisieren" ohne Interface Parameter hinzugefügt
- Neue Klasse "NewGameManager" hinzugefügt
- Methode "DauerPlusEins" der Klasse "AktiveSpionagen" geändert in "DauerReduzieren"

**[EN]**
- Changed Project to .NET Standard 2.0
- Added a new overload method for "Initialisieren" without interface parameters
- Added new class "NewGameManager"
- Changed method "DauerPlusEins" of class "AktiveSpionagen" in "DauerReduzieren"

## 2.3.0

_23.12.2023_

**[DE]**
- Mit der Auslagerung der Kartenspiel Logik in neue Klasse "Kartenspiel" begonnen
- Bugfix von "PrivilegienAktualisieren", es setzt "Amt niederlegen" nun wieder nur dann auf true, wenn der Spieler auch ein Amt bekleidet

**[EN]**
- Started with the outsourcing of the card game logic to the new class "Kartenspiel"
- Bugfix for "PrivilegienAktualisieren", it sets "Amt niederlegen" now correct only to true, if the player has an office

## 2.2.0

_06.12.2022_

**[DE]**
- Die Strafe "Einen Monat Kerker" hat nun eine variable Länge, abhängig von der Schwere der Schuld. Die Auswirkung auf die Gesundheit des verurteilten Spielers ist dann entsprechend höher.
- Methode "TestamentVollstrecken" hinzugefügt (ausgelagert aus WinForms Client)
- Rohstoffrechte (Handelszertifikate) werden nun bei jeder Amtsverleihung sowie Kauf einer Zollburg oder eines Räuberlagers gewährt und sind nicht mehr abhängig von Talergrenzen sowie nicht mehr beschränkt auf maximal 5.
- Bei Spielen mit nur einem aktiven Spieler erhöhen Warenverkäufe des Spielers an die Stadt am Ende der Runde nun korrekt den Lagerstand der Stadt
- Feste benötigen die Waren nun im Lager der Stadt anstelle im Lager der Niederlassung des Spielers. Somit ist es nun überhaupt erst möglich, alle Waren für die größeren Feste zu beschaffen (sofern die Handelszertifikate dafür vorhanden sind).
- Die von den Einwohnern einer Stadt verbrauchten Waren am Rundenende wurden reduziert
- Kleinere Optimierungen

## 2.1.1

_30.10.2022_

**[DE]**
- Beim Privileg "Jurist aufsuchen" wird nun ein Rechtsklick im Dialog nicht mehr als "Ja" sondern als Abbruch gewertet
- Die Vergabe von Titeln wurde neu balanciert und ist nun u.a. auch vom Wohnsitz sowie vom Besitz militärischer Stützpunkte abhängig, zusätzlich wurde die Talergrenze der höheren Titel herabgesetzt. Dafür wurde die Vergabelogik in die einzelnen Titel-Klassen ausgelagert sowie etwas aufgeräumt und optimiert.

## 2.1.0

_23.10.2022_

**[DE]**
- Bei der Berechnung des Gesamtvermögens eines Spielers werden Rohstoffe nun nicht anhand des Standardpreises sondern des aktuellen Preises in der Stadt, in der sie gelagert sind, berechnet
- Neue Klasse für "Kupplerin" hinzugefügt
- Debug ToString Methoden zu verschiedenen Spielerklassen hinzugefügt, um Fehler einfacher finden zu können und einen besseren Überblick über die undurchsichtige Array Struktur mancher Objekte zu bekommen
- Titelstufe von Fürst/Fürstin und Herzog/Herzogin vertauscht, um sie an den historischen Rang in Europa anzupassen

## 2.0.0

_23.12.2021_

**[DE]**
- Der Preis für die Adoption eines Waisenkindes wurde etwas erhöht und wird nun korrekt vom Vermögen des Spielers abgezogen. Zusätzlich kostet eine Adoption nun Ansehen (Balancing).
- Chance reduziert, von KI-Gegnern auf den Einstellungen niedrig und mittel angeklagt zu werden
- Höhe der Geldstrafe vor Gericht reduziert
- Wenn ein Spieler entfernt wird (egal ob durch Tod, Aufgabe oder manuelles Hinauswerfen):
  - Dann werden nun seine Stützpunkte an zufällige KI-Spieler verteilt
  - Dann behalten nachrückende Spieler nun ihre Stützpunkte, Beziehungen zu den KI-Spielern und Amtsinformationen im jeweiligen Gebiet
- Das Minimum für maximale Anwesen (Finanzgesetz) wurde von 1 auf 4 erhöht

## 1.3.0

_24.12.2020_

**[EN]**
- Added new game settings class
- Added new "Impeachment" penalty
- Added new method for creating court hearings from AI players

**[DE]**
- Neue Klasse für Spieleinstellungen hinzugefügt
- Neue Strafe "Amtsenthebung" hinzugefügt
- Neue Methode für die Ermittlung von Anklagen von KI-Spielern hinzugefügt

## 1.2.0

_21.12.2020_

**[EN]**
- Increased chance of being charged by AI opponents
- Adjusted the amount of the penalties for court defeat, they now partly depends on the severity of the guilt

**[DE]**
- Chance erhöht, von KI-Gegnern angeklagt zu werden
- Höhe der Strafen bei Gerichtsniederlage angepasst, sie sind teilweise nun auch von der Schwere der Schuld abhängig

## 1.1.0

_15.10.2020_

**[EN]**
- New privilege added: Visit lawyer

**[DE]**
- Neues Privileg hinzugefügt: Jurist aufsuchen

## 1.0.0

_09.10.2020_

**[EN]**
### Enhancements
- First automatically published version of the library

**[DE]**
### Erweiterungen
- Erste automatisch veröffentlichte Version der Bibliothek

## 0.9.0

_08.10.2020_

**[EN]**
### Enhancements
- First version of the library

**[DE]**
### Erweiterungen
- Erste Version der Bibliothek
