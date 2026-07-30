# Changelog Conspiratio.Lib

## 3.53.0

_28.07.2026_

**[DE]**
- Räuber/Söldner-System (Issue #16): Ein vor dem Kampf bezahlter Moral-Bonus wird nun mit dem Kampf verbraucht und bei einem Sieg **nicht** zurückerstattet (zuvor gab es bei Sieg eine Rückerstattung). Nur ein ungenutzter Bonus (ohne stattgefundenen Kampf) wird weiterhin erstattet.

**[EN]**
- Robbers/mercenaries system (issue #16): a morale bonus paid before combat is now consumed with the combat and is **no longer** refunded on victory (previously victory triggered a refund). Only an unused bonus (without a combat that took place) is still refunded.

## 3.52.0

_28.07.2026_

**[DE]**
- Räuber/Söldner-System (Issue #16): Vor dem Kampf kann für die Truppen eines Stützpunkts ein einmaliger Moral-Bonus bezahlt werden (`Stuetzpunkt.MoralBonusZahlen`, Kosten je Truppenstärke; `MoralBonusBezahlt`, `BerechneKostenMoralBonus`). Der Bonus hebt die Kampfmoral der Angreifer um `MoralBonusWert` (15 %-Punkte, siehe `MoralFuerKampf`, im Kampfaufbau berücksichtigt) und wird bei einem Sieg zurückerstattet, sonst verfällt er; ungenutzte Boni (ohne Kampf) werden ebenfalls erstattet (`KampfereignisseManager`).

**[EN]**
- Robbers/mercenaries system (issue #16): before combat a one-time morale bonus can be paid for a base's troops (`Stuetzpunkt.MoralBonusZahlen`, cost by troop strength; `MoralBonusBezahlt`, `BerechneKostenMoralBonus`). The bonus raises the attackers' combat morale by `MoralBonusWert` (15 percentage points, see `MoralFuerKampf`, applied when building the combat) and is refunded on victory, otherwise it is forfeited; unused bonuses (without a combat) are refunded as well (`KampfereignisseManager`).

## 3.51.0

_28.07.2026_

**[DE]**
- Räuber/Söldner-System (Issue #16): Die Aktivität der KI-Spieler in den Militärstützpunkten wird nun über einen feinen Prozentwert gesteuert (`Spieleinstellungen.KiAktivitaetProzent`, 1–100, Standard 50) statt der bisherigen drei Stufen. Räuberlager und Zollburg leiten ihren Aktivitätsfaktor direkt daraus ab (50 % = bisheriger Normalwert); alte Spielstände (Wert 0) werden wie 50 % behandelt.

**[EN]**
- Robbers/mercenaries system (issue #16): the activity of the AI players in the military bases is now controlled via a fine percentage value (`Spieleinstellungen.KiAktivitaetProzent`, 1–100, default 50) instead of the previous three levels. Robber camps and toll castles derive their activity factor directly from it (50 % = the previous normal value); old savegames (value 0) are treated as 50 %.

## 3.50.0

_28.07.2026_

**[DE]**
- Räuber/Söldner-System (Issue #16): Die von menschlichen Zollburg-Besitzern eingenommenen Zölle werden nun mitgezählt (`HumSpieler.ZolleinnahmenGesammelt`, akkumuliert in der Abrechnung) und dem Spieler zu Zugbeginn als Einnahme gemeldet (`SoeldnerRaeuberManager.ZeigeZolleinnahmen`), danach zurückgesetzt.

**[EN]**
- Robbers/mercenaries system (issue #16): the tolls collected by human toll-castle owners are now tallied (`HumSpieler.ZolleinnahmenGesammelt`, accumulated during settlement) and reported to the player as income at the start of the turn (`SoeldnerRaeuberManager.ZeigeZolleinnahmen`), then reset.

## 3.49.0

_28.07.2026_

**[DE]**
- Räuber/Söldner-System (Issue #16): Ein eigener Stützpunkt kann nun zum Verkauf angeboten werden (`Stuetzpunkt.ZumVerkaufAngeboten`, `StuetzpunktVerwaltenManager.ZumVerkaufAngeboten`, `SoeldnerRaeuberManager.SetzeZumVerkauf`/`IstZumVerkaufAngeboten`). Zu Zugbeginn unterbreiten KI-Spieler dann gelegentlich zufällige Kaufangebote (`GeneriereKiKaufangebote`, Preis um den aktuellen Wert), die dem Besitzer vorgelegt werden. `Stuetzpunkt.AngebotVorlegen` verarbeitet jetzt auch KI-Anbieter: bei Annahme wechselt der Stützpunkt gegen Bezahlung zur KI (Verkaufs-Flag wird zurückgesetzt), bei Ablehnung bleibt er gelistet.

**[EN]**
- Robbers/mercenaries system (issue #16): an own base can now be put up for sale (`Stuetzpunkt.ZumVerkaufAngeboten`, `StuetzpunktVerwaltenManager.ZumVerkaufAngeboten`, `SoeldnerRaeuberManager.SetzeZumVerkauf`/`IstZumVerkaufAngeboten`). At the start of the turn AI players then occasionally submit random purchase offers (`GeneriereKiKaufangebote`, price around the current value) that are presented to the owner. `Stuetzpunkt.AngebotVorlegen` now also handles AI bidders: on acceptance the base changes to the AI for payment (the for-sale flag is reset), on rejection it stays listed.

## 3.48.0

_28.07.2026_

**[DE]**
- Räuber/Söldner-System (Issue #16): Der Anbieter eines Stützpunkt-Kaufangebots erhält nun zu Beginn seines nächsten Zuges eine eigene Meldung über das Ergebnis (Annahme durch den Besitzer inkl. Besitzwechsel bzw. Ablehnung mit Rückerstattung des reservierten Betrags). Dazu Nachrichtenliste `HumSpieler.HandelsNachrichten` (lazy initialisiert, spielstandskompatibel) und `SoeldnerRaeuberManager.ZeigeHandelsnachrichten`.

**[EN]**
- Robbers/mercenaries system (issue #16): the bidder of a base purchase offer now receives their own message about the outcome at the start of their next turn (acceptance by the owner incl. change of ownership, or rejection with refund of the reserved amount). Adds the message list `HumSpieler.HandelsNachrichten` (lazily initialized, savegame-compatible) and `SoeldnerRaeuberManager.ZeigeHandelsnachrichten`.

## 3.47.0

_28.07.2026_

**[DE]**
- Räuber/Söldner-System (Issue #16): Kaufangebote für Stützpunkte können nun auch an menschliche Mitspieler gerichtet werden. Das Angebot reserviert den Betrag beim Anbieter und wird dem Besitzer zu Beginn seines nächsten Zuges vorgelegt (`Stuetzpunkt.AngebotVorlegen`, `SoeldnerRaeuberManager.VerarbeiteEingehendeKaufangebote`/`StehenKaufangeboteAn`); bei Annahme wechselt der Stützpunkt gegen den Preis den Besitzer (samt Handelszertifikat der Stufe 3), bei Ablehnung wird der reservierte Betrag zurückerstattet. Nebenbei behoben: Der Besitzername im Bestätigungstext bezog sich fälschlich auf den aktiven Spieler statt auf den Stützpunkt-Besitzer.

**[EN]**
- Robbers/mercenaries system (issue #16): purchase offers for bases can now also be directed at human fellow players. The offer reserves the amount from the bidder and is presented to the owner at the start of their next turn (`Stuetzpunkt.AngebotVorlegen`, `SoeldnerRaeuberManager.VerarbeiteEingehendeKaufangebote`/`StehenKaufangeboteAn`); if accepted, the base changes owner for the price (including a tier-3 trade certificate), if declined the reserved amount is refunded. Also fixed: the owner name in the confirmation text wrongly referred to the active player instead of the base owner.

## 3.46.0

_25.07.2026_

**[DE]**
- `StadtInformationenManager` hinzugefügt (Kapselung von StadtInformationen): liefert zu einer Stadt Reichtum, Umsatzsteuer, Einwohner und Kriminalität sowie die Rohstoffangaben (Haupt-/Nebenproduktion, Nachfrage, mögliche Werkstätten) und den Lagerstand des Landes je Rohstoff samt Bewertung (niedrig/normal/hoch)

**[EN]**
- Added `StadtInformationenManager` (encapsulation of StadtInformationen): provides a city's wealth, sales tax, population and crime, plus the resource data (main/secondary production, demand, possible workshops) and the country-wide stock per resource with a rating (low/normal/high)

## 3.45.0

_24.07.2026_

**[DE]**
- `TippsManager` hinzugefügt (Kapselung von TippsAnzeigen): liefert einen zufälligen Start-Tipp, den Text zu einem Index und die Navigation zum nächsten (belegten) bzw. vorherigen Tipp

**[EN]**
- Added `TippsManager` (encapsulation of TippsAnzeigen): provides a random starting tip, the text for an index and navigation to the next (non-empty) resp. previous tip

## 3.44.0

_24.07.2026_

**[DE]**
- `StatistikManager` hinzugefügt (Kapselung von FormStatistik): liefert die menschlichen Spieler samt Banner sowie – je Spieler – die Statistikwerte in zwei Spalten (Beschriftung und formatierter Wert: Hinterzimmer, Kirche, Schreibstube, Handel, Sonstiges sowie Gesamtvermögen und Taler), in der Reihenfolge des Originals

**[EN]**
- Added `StatistikManager` (encapsulation of FormStatistik): provides the human players with their banner as well as – per player – the statistics values in two columns (label and formatted value: back room, church, writing room, trade, other, plus total assets and Taler), in the order of the original

## 3.43.0

_24.07.2026_

**[DE]**
- `LagerraumManager` hinzugefügt (Kapselung von LagerraumKaufen): für eine Werkstätte des Spielers in einer Stadt werden drei Angebote für zusätzlichen Lagerraum (Fläche in m² und Preis) ermittelt – der Preis richtet sich nach dem Reichtum der Stadt; ein Angebot lässt sich bei ausreichendem Guthaben kaufen (erweitert den Lagerraum, zieht den Preis ab)

**[EN]**
- Added `LagerraumManager` (encapsulation of LagerraumKaufen): for a player's workshop in a town, three offers for additional storage space (area in m² and price) are determined – the price depends on the town's wealth; an offer can be bought with sufficient funds (extends the storage, deducts the price)

## 3.42.0

_24.07.2026_

**[DE]**
- `HandelszertifikatManager` hinzugefügt (Kapselung von Handelszertifikat/HandelszertifikatAnzeigen): prüft, ob dem aktiven Spieler ein neues Handelszertifikat zusteht (über den beim Amtsgewinn bzw. Stützpunktkauf gesetzten Vermerk „BekamHandelszertifikat"), und vollzieht die Verleihung – erstellt den Urkundentext samt ausstellendem Rat (Stadt-, Land- oder Reichsrat je nach Tier-Stufe des Rohstoffs) und quittiert den Vermerk

**[EN]**
- Added `HandelszertifikatManager` (encapsulation of Handelszertifikat/HandelszertifikatAnzeigen): checks whether the active player is entitled to a new trade certificate (via the "BekamHandelszertifikat" flag set on winning an office resp. buying a stronghold) and carries out the bestowal – creates the certificate text including the issuing council (town, land or realm council depending on the resource's tier) and clears the flag

## 3.41.0

_23.07.2026_

**[DE]**
- `TitelverleihungErgebnis` um `TitelTyp` (Typ-Name des verliehenen Titels, z. B. "Graf") erweitert – der Client kann damit die passende Sprachausgabe der Titelverleihung auswählen

**[EN]**
- Extended `TitelverleihungErgebnis` with `TitelTyp` (type name of the bestowed title, e.g. "Graf") – the client can use it to select the fitting voice output for the title bestowal

## 3.40.0

_23.07.2026_

**[DE]**
- `TitelVerleihungManager` hinzugefügt (Kapselung von TitelVerleihen/TitelVerleihForm): prüft, ob dem aktiven Spieler ein höherer Titel zusteht (vorgemerkt über das bestehende `VersuchTitelVerleihen`) und ein Regent amtiert, und vollzieht die Verleihung – erstellt den Urkundentext des Regenten, setzt den neuen Adelstitel des Spielers und quittiert den Vermerk. Ohne Regenten wird die Verleihung auf später verschoben

**[EN]**
- Added `TitelVerleihungManager` (encapsulation of TitelVerleihen/TitelVerleihForm): checks whether the active player is entitled to a higher title (flagged via the existing `VersuchTitelVerleihen`) and a regent is in office, and carries out the bestowal – creates the regent's decree text, sets the player's new noble title and clears the flag. Without a regent the bestowal is deferred

## 3.39.0

_23.07.2026_

**[DE]**
- `GerichtsverhandlungManager` hinzugefügt (Kapselung von CheckGerichtsVerhandlungen/GerichtsverhandlungDurchfuehren): schrittweiser Ablauf einer Verhandlung, damit der Client die Anzeige steuern und menschliche Richter interaktiv abstimmen lassen kann – Verhandlungen mit dem aktiven Spieler ermitteln, Verhandlung starten (Delikte des Angeklagten bestimmen: bei KI zufällig, bei Mensch aus den begangenen Verbrechen), die Vorwürfe je Gesetzeskategorie (Finanz/Straf/Kirche) mit passender Überschrift liefern, die drei Richter abstimmen lassen (KI-Urteil aus Sympathie, Verbrechenssumme und Schwierigkeitsgrad) und auswerten (bei mehr als einem Freispruch Freispruch, sonst eine zufällige Strafart ausführen)

**[EN]**
- Added `GerichtsverhandlungManager` (encapsulation of CheckGerichtsVerhandlungen/GerichtsverhandlungDurchfuehren): a step-by-step trial flow so the client can drive the display and let human judges vote interactively – determine the trials involving the active player, start a trial (determine the defendant's delicts: random for an AI, from the committed crimes for a human), return the charges per law category (financial/criminal/church) with a fitting heading, let the three judges vote (AI verdict from sympathy, crime sum and difficulty) and evaluate (acquittal on more than one not-guilty vote, otherwise execute a random punishment)

## 3.38.0

_23.07.2026_

**[DE]**
- `Kartenspiel` um die Spiellogik von "17 und 4" erweitert (Migration von KartenSpielen aus dem WinForms-Client): Einsatz festlegen (Min = 5 % des Vermögens, Max = Bargeld), Austeilen (Gegner eine Karte, Spieler zwei), Karten kaufen mit Status (weiter / genau 21 / überkauft) und die Auswertung – der Gegner zieht bis mindestens 17, danach werden Taler und die Beziehung zum Gegner verbucht. Ebenso der Fall zu geringer Taler (der Gegner verlässt verärgert den Tisch). Behebt beiläufig einen Kopier-Fehler des Originals, durch den die zweite Startkarte des Spielers nie ausgelost wurde

**[EN]**
- Extended `Kartenspiel` with the "17 and 4" game logic (migration of KartenSpielen from the WinForms client): setting the bet (min = 5 % of wealth, max = cash), dealing (one card for the opponent, two for the player), buying cards with a status (continue / exactly 21 / bust) and the evaluation – the opponent draws until at least 17, after which Taler and the relationship to the opponent are booked. Likewise the too-few-Taler case (the opponent leaves the table annoyed). Fixes in passing a copy-paste bug of the original that meant the player's second starting card was never drawn

## 3.37.0

_23.07.2026_

**[DE]**
- `ZufallsereignisseManager` hinzugefügt (Extraktion von RandomEreignisse aus dem WinForms-Client): wickelt die jährlichen Zufallsereignisse eines Zugs ab – je ein zufälliges Finanz-, Ansehens- und Gesundheitsereignis (mit vermögensabhängigem Taler-, Ansehens- bzw. Gesundheitseffekt) sowie ein datengesteuertes Datumsereignis mit Multiplikatoren –, verbucht deren Auswirkungen und liefert die anzuzeigenden Meldungen der Reihe nach zurück (im Startjahr passiert nichts)

**[EN]**
- Added `ZufallsereignisseManager` (extraction of RandomEreignisse from the WinForms client): handles a turn's yearly random events – one random financial, reputation and health event each (with a wealth-based Taler, reputation resp. health effect) as well as a data-driven date event with multipliers –, applies their effects and returns the messages to be shown in order (nothing happens in the starting year)

## 3.36.0

_23.07.2026_

**[DE]**
- `ZugNachrichtenManager` um die verdeckten Zugende-Ereignisse erweitert (Extraktion aus dem WinForms-Client): Korruptionsgelder (Privileg 21) und Schmuggelgelder (Privileg 22) als Amtseinkünfte, Kerkerklatsch (Privileg 7, Beweise gegen einen Amtsträger der Amtsstadt), die Abwicklung laufender Spionagen (Dauer, Ablauf, Beweisbeschaffung abhängig von Deliktpunkten und Schutzprivilegien) und Sabotagen (vermögensabhängiger Schaden mit Chance) sowie die Ausführung einer beauftragten Ermordung und eines beauftragten vergifteten Weins (je mit Erfolgschance und Zustandsänderung)

**[EN]**
- Extended `ZugNachrichtenManager` with the covert turn-end events (extraction from the WinForms client): corruption money (privilege 21) and smuggling money (privilege 22) as office earnings, dungeon gossip (privilege 7, evidence against an office holder of the office city), the processing of ongoing espionage (duration, expiry, evidence gathering depending on delict points and protective privileges) and sabotage (wealth-based damage with a chance) as well as carrying out a commissioned assassination and a commissioned poisoned wine (each with a success chance and state change)

## 3.35.0

_21.07.2026_

**[DE]**
- `KampfereignisseManager` hinzugefügt (Kapselung von frmKampfereignisse): wickelt die militärischen Ereignisse am Jahresende ab – führt die Aktionen der KI-Stützpunkte aus, initialisiert die Landsicherheiten, ermittelt und wertet die stattfindenden Kämpfe aus (Karawanen-Plünderungen, Angriffe auf Stützpunkte) – und liefert die Meldungen der Reihe nach zurück

**[EN]**
- Added `KampfereignisseManager` (encapsulation of frmKampfereignisse): handles the year-end military events – executes the AI strongholds' actions, initializes the land securities, determines and resolves the battles that take place (caravan plundering, attacks on strongholds) – and returns the messages in order

## 3.34.0

_21.07.2026_

**[DE]**
- `StuetzpunktAktionenManager` hinzugefügt (Kapselung des Aktionsbereichs von frmStuetzpunktVerwalten): die beiden Auftrags-Slots eines Stützpunkts – Aktionsart (Kein Auftrag / Truppen schicken → anderer Stützpunkt / Überwachen bzw. Plündern → Grafschaft), Ziel und Einheiten-Zuteilung samt der Obergrenze je Einheit (stationierte Truppen minus Zuteilung des anderen Slots)

**[EN]**
- Added `StuetzpunktAktionenManager` (encapsulation of frmStuetzpunktVerwalten's action area): the two order slots of a stronghold – action type (no order / send troops → another stronghold / monitor resp. plunder → county), target and unit allocation including the per-unit upper limit (stationed troops minus the other slot's allocation)

## 3.33.0

_21.07.2026_

**[DE]**
- `StuetzpunktVerwaltenManager` hinzugefügt (Kapselung des Kernbereichs von frmStuetzpunktVerwalten): die vier Einheitentypen eines eigenen Stützpunkts (je nach Zollburg oder Räuberlager) mit Plural-Name und Anzahl, das Anheuern/Entlassen von Truppen sowie das Manöver. Sicherheit/Tarnung, Zustand, Kapazität und Zollsatz laufen über den bestehenden Prozentwert-festlegen-Dialog

**[EN]**
- Added `StuetzpunktVerwaltenManager` (encapsulation of frmStuetzpunktVerwalten's core area): the four unit types of one's own stronghold (depending on toll castle or robber camp) with plural name and count, the recruiting/dismissing of troops and the maneuver. Security/camouflage, condition, capacity and toll rate go through the existing set-percentage dialog

## 3.32.0

_21.07.2026_

**[DE]**
- `SoeldnerRaeuberManager` hinzugefügt (Kapselung von frmSoeldnerRaeuberKarte/frmStuetzpunktKaufen): liefert die acht Stützpunkte (Zollburgen und Räuberlager) mit ihren Kartenrechtecken und Besitzverhältnissen (für Hover und Flaggen), die Anzeigedaten für den Kauf-Dialog (Name, Art, Besitzer, Wert, Zustand, Sicherheit/Tarnung) sowie das jährlich einmalige Kaufangebot an fremde Besitzer

**[EN]**
- Added `SoeldnerRaeuberManager` (encapsulation of frmSoeldnerRaeuberKarte/frmStuetzpunktKaufen): provides the eight strongholds (toll castles and robber camps) with their map rectangles and ownership (for hover and flags), the display data for the purchase dialog (name, type, owner, value, condition, security/camouflage) and the once-a-year purchase offer to other owners

## 3.31.0

_21.07.2026_

**[DE]**
- `DynamischeSpieldaten.Anschwaerzen(id)` samt `GetAnschwaerzID`/`SetAnschwaerzID` hinzugefügt (Migration des zweistufigen Anschwärzens aus dem Hinterzimmer): der erste Klick wählt den Anzuschwärzenden X, der zweite den Adressaten Y. Glaubt Y (nur KI, ab Beziehung 80) die Anschuldigung, sinkt dessen Beziehung zu X; andernfalls berichtet Y dem X davon und die eigene Beziehung leidet. `KontrahentenManager.PersonWasMachen` ruft dies für Modus 2 auf

**[EN]**
- Added `DynamischeSpieldaten.Anschwaerzen(id)` with `GetAnschwaerzID`/`SetAnschwaerzID` (migration of the back room's two-step denunciation): the first click selects the denounced X, the second the addressee Y. If Y (AI only, from relationship 80) believes the accusation, its relationship to X drops; otherwise Y reports it to X and one's own relationship suffers. `KontrahentenManager.PersonWasMachen` invokes this for mode 2

## 3.30.0

_21.07.2026_

**[DE]**
- `DynamischeSpieldaten.KartenSpielen(id)` und `DynamischeSpieldaten.Bestechen(id, wert)` hinzugefügt (Migration von BeziehungenPflegen/Bestechen aus dem Hinterzimmer): Karten spielen (nur gegen KI – bei genügend eigenem Reichtum sagt sie zu, sonst sinkt die Beziehung) und Bestechen (eine Taler-Summe zukommen lassen, mit Statistik und Gesetzesprüfung). `KontrahentenManager.PersonWasMachen` ruft für Modus 0 den Beziehungen-pflegen-Dialog über `SW.UI.BeziehungPflegen` auf

**[EN]**
- Added `DynamischeSpieldaten.KartenSpielen(id)` and `DynamischeSpieldaten.Bestechen(id, wert)` (migration of BeziehungenPflegen/Bestechen from the back room): playing cards (only against AI – with enough own wealth it agrees, otherwise the relationship drops) and bribing (giving a Taler sum, with statistics and law check). `KontrahentenManager.PersonWasMachen` invokes the relations dialog via `SW.UI.BeziehungPflegen` for mode 0

## 3.29.0

_21.07.2026_

**[DE]**
- `DynamischeSpieldaten.Spionage(id)` hinzugefügt (Migration der Spionage-Form aus dem Hinterzimmer): läuft noch keine Spionage, werden – wie im Original ohne Rückfrage – für 5 Jahre Spione angesetzt (Kosten 2 % der Taler des Ziels, mind. 1.000 Taler) und darüber informiert; läuft bereits eine, kann man sie zurückpfeifen oder weiterlaufen lassen. `KontrahentenManager.PersonWasMachen` ruft dies für Modus 3 auf

**[EN]**
- Added `DynamischeSpieldaten.Spionage(id)` (migration of the back room's spying form): if no spying is running yet, spies are set on the target for 5 years – as in the original without a confirmation – (cost 2 % of the target's Taler, at least 1,000 Taler) and reported via an info message; if one is already running, it can be called off or left to continue. `KontrahentenManager.PersonWasMachen` invokes this for mode 3

## 3.28.0

_21.07.2026_

**[DE]**
- `DynamischeSpieldaten.Sabotage(id)` hinzugefügt (Migration der Sabotage-Form aus dem Hinterzimmer): läuft noch keine Sabotage gegen das Ziel, wird sie gegen jährliche Kosten (4 % des Zielvermögens, mind. 1.000 Taler, über 5 Jahre) mit Rückfrage eingeleitet; läuft bereits eine, kann man sie zurückpfeifen oder weiterlaufen lassen. `KontrahentenManager.PersonWasMachen` ruft dies für Modus 1 auf

**[EN]**
- Added `DynamischeSpieldaten.Sabotage(id)` (migration of the back room's sabotage form): if no sabotage against the target is running yet, it is initiated for an annual cost (4 % of the target's wealth, at least 1,000 Taler, over 5 years) with a confirmation; if one is already running, it can be called off or left to continue. `KontrahentenManager.PersonWasMachen` invokes this for mode 1

## 3.27.0

_21.07.2026_

**[DE]**
- `KontrahentenManager.PersonWasMachen` um den Hinterzimmer-Modus 4 (Ermordung, `SW.Dynamisch.Ermordung`) erweitert; noch nicht migrierte Modi (Beziehungen, Sabotage, Anschwärzen, Spionage, Erpressung) liefern vorerst einen Hinweis "noch nicht verfügbar"

**[EN]**
- Extended `KontrahentenManager.PersonWasMachen` with the back-room mode 4 (assassination, `SW.Dynamisch.Ermordung`); not-yet-migrated modes (relations, sabotage, denunciation, spying, blackmail) return a "not yet available" notice for now

## 3.26.0

_21.07.2026_

**[DE]**
- `AemterEbeneManager`: `AemterSlotInfo` um die Statusdaten der Amtsinhaber erweitert – laufende Sabotage/Spionage des aktiven Spielers gegen den Inhaber, Ehe-Status, Konfession (`AemterKonfession`) sowie ob es ein KI-Spieler ist und dessen Beziehung zum aktiven Spieler (0–100). Dient den Symbol-Overlays und dem Beziehungs-Balken der Ämter-Ebene

**[EN]**
- `AemterEbeneManager`: extended `AemterSlotInfo` with the office holders' status data – the active player's running sabotage/spying against the holder, marital status, confession (`AemterKonfession`) as well as whether it is an AI player and its relationship to the active player (0–100). Feeds the office layer's symbol overlays and relationship bar

## 3.25.0

_21.07.2026_

**[DE]**
- `AemterEbeneManager` hinzugefügt (Kapselung der Ämter-Struktur von AemterEbene): liefert für ein Gebiet (Stadt/Land/Reich = Stufe 0/1/2) je politischer, kirchlicher und militärischer Ebene die Ämter mit ihren Inhabern (generisch über `Gebiet.GetAmtX` und die statischen Amt-IDs, geschlechtsabhängige Amtsnamen) sowie die Titelzeile und die zielgerichtete Aktion (Prozess/Henkershand)

**[EN]**
- Added `AemterEbeneManager` (encapsulation of AemterEbene's office structure): for a territory (town/land/realm = level 0/1/2) it provides the offices with their holders for the political, church and military layer (generically via `Gebiet.GetAmtX` and the static office IDs, with gender-aware office names) as well as the title line and the targeted action (lawsuit/executioner's hand)

## 3.24.0

_21.07.2026_

**[DE]**
- `KontrahentenManager` hinzugefügt (Kapselung von KontrahentenForm/UI.PersonWasMachen für die Personen-Ziel-Privilegien der Weltkarte): liefert die wählbaren Kontrahenten (menschliche Mitspieler zuerst, dann die KI) und führt die zielgerichtete Aktion aus – Modus 8 = Prozess initiieren, Modus 13 = Hand des Henkers (mit Selbst-Schutz und Leer-Guard)

**[EN]**
- Added `KontrahentenManager` (encapsulation of KontrahentenForm/UI.PersonWasMachen for the world map's person-targeting privileges): provides the selectable contestants (human players first, then the AI) and performs the targeted action – mode 8 = initiate a lawsuit, mode 13 = executioner's hand (with a self-protection and empty guard)

## 3.23.0

_21.07.2026_

**[DE]**
- `RohstoffpreiseManager` hinzugefügt (Kapselung von RohstoffpreiseForm für die Weltkarte-Modi Händler/Kaufmann/Großkaufmann): Rohstoffpreise einer Stadt sowie die level-abhängige Aktion beim Anklicken eines Rohstoffs (Level 0 Einsicht, Level 1/2 Preis-Beeinflussung mit geteilter Einmal-pro-Jahr-Sperre). Die Original-Eigenheit, dass die Preisänderung nur erzählt, der Grundpreis aber nicht verändert wird, bleibt erhalten

**[EN]**
- Added `RohstoffpreiseManager` (encapsulation of RohstoffpreiseForm for the world map modes merchant/trader/grand merchant): a town's raw material prices and the level-dependent action when clicking a raw material (level 0 view-only, level 1/2 price influence with a shared once-per-year lock). The original quirk that the price change is only narrated but the base price is not actually altered is preserved

## 3.22.0

_21.07.2026_

**[DE]**
- `UntergebeneManager` hinzugefügt (Kapselung von UntergebeneForm/UntergebenenOptionen): liefert die Untergebenen des aktiven Spielers (durch dessen Amt bestimmt), die Optionsfrage sowie die einzige echte Aktion – die Einleitung einer Amtsenthebung samt Ergebnismeldung

**[EN]**
- Added `UntergebeneManager` (encapsulation of UntergebeneForm/UntergebenenOptionen): provides the active player's subordinates (determined by their office), the options prompt and the only real action – initiating a dismissal from office including the result message

## 3.21.0

_21.07.2026_

**[DE]**
- `ProzentwertFestlegenManager` hinzugefügt (Kapselung von ProzentwertFestlegenForm für alle fünf Prozentwert-Arten): Anzeige-Texte, NumericButton-Konfiguration (Min/Start/Max/Stellen), Live-Übernahme bei Umsatzsteuer und Zollsatz sowie Kostenberechnung und Auftragsausführung bei den Stützpunkt-Verbesserungen (Sicherheit/Tarnung, Zustand, Kapazität)

**[EN]**
- Added `ProzentwertFestlegenManager` (encapsulation of ProzentwertFestlegenForm for all five percentage types): the display texts, the NumericButton configuration (min/start/max/digits), live application for sales tax and toll rate, and the cost calculation plus order execution for the stronghold improvements (security/camouflage, condition, capacity)

## 3.20.0

_21.07.2026_

**[DE]**
- `BauwerkStiftenManager` hinzugefügt (Kapselung von BauwerkStiftenForm): stiftbare Bauwerke samt Preisen, zyklische Städtewahl, Bezahlbarkeits-Prüfung ohne Nebenwirkung, Bestätigungsfrage und die eigentliche Stiftung (Permaansehen erhöhen, Taler abziehen)

**[EN]**
- Added `BauwerkStiftenManager` (encapsulation of BauwerkStiftenForm): the buildings that can be donated with their prices, cyclic town selection, a side-effect-free affordability check, the confirmation prompt and the donation itself (raising the permanent reputation, deducting the Taler)

## 3.19.0

_18.07.2026_

**[DE]**
- `FestManager.FeiereFaelligesFest` hinzugefügt: feiert das für den aktiven Spieler im aktuellen Jahr geplante Fest (falls vorhanden), entfernt es aus der Planung und liefert die Ergebnismeldung – kapselt die Feste-Abfrage, die bisher im Frontend liegen müsste

**[EN]**
- Added `FestManager.FeiereFaelligesFest`: celebrates the feast planned for the active player in the current year (if any), removes it from the planning and returns the result message – encapsulating the feast lookup that would otherwise live in the frontend

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
