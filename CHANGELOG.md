# Changelog Conspiratio.Lib

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
