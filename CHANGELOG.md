# Changelog Conspiratio.Lib

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
