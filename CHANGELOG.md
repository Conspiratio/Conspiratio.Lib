# Changelog Conspiratio

## Unreleased

### Erweiterungen
- Neue Aktion in der Kirche für "Waisenkind adoptieren" hinzugefügt. Damit kann mithilfe einer großzügigen Spende ein Mündel adoptiert werden, sofern man selbst noch kein Kind hat. Damit soll das frustrierende und unvermeidliche Spielende bei plötzlichem Kindstod und anschließendem Tod des Spielercharakters verhindert werden.

### Änderungen
- .NET Framework 4.6.2 ist nun Voraussetzung für Conspiratio (vorher 4.5)

### Korrekturen
- Bei Zollburgen oder Räuberlagern, die im Besitz eines menschlichen Spielers sind, wird der Name des Besitzers nun immer korrekt im "Kaufen"-Fenster des Stützpunktes angezeigt. Vorher wurde hier immer der Spieler angezeigt, der gerade an der Reihe war.

## 1.4.2 - 24.12.2019

### Erweiterungen
- Neues Privileg "Ein Fest veranstalten" in einer ersten Version hinzugefügt (es sind noch Änderungen und Erweiterungen geplant)
- 'Geld zum Fenster rauswerfen' Button im Kontor hinzugefügt, um als aktiver Spieler aus dem Spiel auszuscheiden
- Button zum Beenden der Runde im Kontor hinzugefügt (das dürfte für viele Spieler intuitiver sein als der Rechtsklick in einem freien Bereich, der aber natürlich nach wie vor funktioniert)
- Spezielle Ereignisse hinzugefügt (welche genau, werdet ihr selbst herausfinden müssen ...)

### Korrekturen
- "Schmilzt Kupfer" in "Schmelzt Kupfer" geändert (Danke an sknortsch für den Hinweis)
- Anzeigefehler: Verlorene Waren aufgrund von Überfällen werden im Buch bei Rundenbeginn nun nur noch einmal angezeigt und nicht mehr jede Runde ab dem Überfall (Danke an sknortsch für den Hinweis)
- Im Setup wird nun wieder die aktuelle Datei "Produktion.pdf" ausgeliefert.
- Doppelte Klicksounds an einigen Stellen korrigiert (z.B. beim Kirchgang)
- Wenn ein Spieler aus dem Spiel ausscheidet (bei Tod keinen Erben oder Spieler hinauswerfen) und es gibt noch weitere Spieler, dann kommt es beim Beenden eines solches Spiels mit Anzeige der Statistik nicht mehr zu einer Fehlermeldung
- Wenn ein Spieler durch 'Spieler hinauswerfen' oder 'Geld zum Fenster rauswerfen' aus dem Spiel ausscheidet, dann wird nun auch sein möglicherweise vorhandenes Amt freigegeben und eine eventuelle Wahlteilname gelöscht
- Neues Spiel > Spieler hinzufügen: Der Wappen-Cursor wird nun nicht mehr nach dem ersten Spieler bei der Erstellung aller weiterer Spieler angezeigt
- Neues Spiel > Spieler hinzufügen: Es wird nun nicht mehr das Eingabefeld für den Namen angezeigt, wenn der letzte Spieler bereits erstellt wurde und die Spielerstellung abgeschlossen ist und nur noch mit Rechtsklick bestätigt werden muss
- Diverse Rechtschreib- und Tippfehler korrigiert

## 1.4.1 - 24.12.2018

### Erweiterungen
- Neuen Button zum Spiel fortsetzen hinzugefügt, es wird dann das zuletzt gespeicherte Spiel geladen (falls vorhanden). Ist kein zuletzt gespeichertes Spiel vorhanden, ist der Button deaktiviert.
- Weitere Tipps zu Räuber & Söldner sowie den neuen Werbungsgeschenken hinzugefügt.
- Es gibt nun bei Linksklick auf Links und Buttons und bei Rechtsklick zum Schließen von Fenstern einen entsprechenden Sound, der zur Atmosphäre beitragen soll (Danke an sknortsch für die Anregung)

### Änderungen
- In den Ankündigungsfenstern für neuen Titel und neues Handelszertifikat wurde das Hintergrundbild getauscht sowie einen Fanfare Sound eingebaut.
- Optimierung der Spielerstellung, man kann nun mit ESC zurück gehen, um eine Eingabe zu verändern. Wird bei der Eingabe des Spielernamens ESC gedrückt, dann wird die Spielerstellung komplett abgebrochen und man landet im Hauptmenü.
- Im Fenster für neues Spiel ist nun der Fokus beim Start im Eingabefeld und bei einem Druck auf Enter/Eingabe wird das Spiel direkt erstellt.
- Im Hintergrund wurde die Art und Weise verändert, wie der Spielstand gespeichert und geladen wird, um u.a. stabiler und auch flexibler für zukünftige Erweiterungen zu sein. Generell sollte nun jegliche Information des Spiels sauber festgehalten und wiederherstellt werden. Sollten euch hier noch Ungereimtheiten auffallen, bitte im Forum melden.
- Im Fenster zur Auswahl eines Wohnsitzes (beim Bau/Umbau) werden nun die Kosten mit Tausenderpunkten formatiert und sind mit dem Zusatz "Taler" versehen

### Korrekturen
- Wenn man bei Rundenende nicht genug Geld fürs Kartenspielen hatte, dann wurde die Aktion in den Folgejahren solange wiederholt, bis das Spiel stattfinden konnte. Nun passiert es nur noch einmal.
- Wenn ein Kartenspiel solange geht, dass der Text geleert werden muss und eine neue "Seite" angefangen wird, dann wurde der Spieler nicht gefragt, ob er noch eine Karte ziehen will sondern es wurden nur die Buttons ohne Frage angezeigt, Der Text für die Frage wurde in diesem Fall nun ergänzt.
- Anrede bei weiblichen Spielern an diversen Stellen korrigiert, weibliche Ämterbezeichnungen teilweise angepasst.
- In einer Stadt gekaufte Werkstätten für Medizin inkl. Lagerraum gehen nun beim Speichern nicht mehr verloren (Danke an AnLa für den Hinweis).
- Installation: Es wurde das Setup-Tool gewechselt, um endlich alle Schönheitsfehler in den Griff zu bekommen. Es wird nun nicht mehr der Ordner Conspiratio im Standardpfad doppelt vorgeschlagen.
- Installation: Ein abweichenden Standardpfad im Installer wird nun korrekt berücksichtigt (Danke an Petrusilius für den Hinweis).
- Verschiedene kleinere Textkorrekturen

## 1.4.0 - 05.08.2018

### Erweiterungen:

- Ein vollständiges Räuber & Söldner System steht zum Spielen bereit.
- Die Militärkarte mit Übersicht über alle Zollburgen und Räuberlager ist ab sofort im Kontor bei den Schwertern abrufbar.
- Ein neues Verwaltungsfenster zur Verwaltung von Räuberlager und Söldnerburgen.
- Neuer Ereignisbildschirm bei Rundenende für militärische Ereignisse. Hier werden auch die Kämpfe in Dialogform abgewickelt.
- Neue Zeile in der Abrechnung für Sold (Bezahlung der Truppen in einer Zollburg oder einem Räuberlager).
- Einbau von Tooltips für Religions Icons im Bildschirm für neuen Spieler.

### Änderungen:

- Taler sind nun überall einheitlich mit Tausendertrennzeichen zur besseren Lesbarkeit formatiert.
- Wenn das Spiel auf einen Rechtsklick wartet, so ist dies nun durch einen anderen Mauszeiger (Spielerwappen) erkennbar.
- Brautwerbung: 20 neue Geschenke in drei verschiedenen Stufen und Preisklassen, die die Partnersuche erleichtern sollen.
- Credits

### Korrekturen:

- Korrektur des Klickbereichs der Handelskarte für den Bildschirm "Handel". Er ist nun etwas größer und deckt die gesamte Karte an der Wand im Kontor ab.
- Unbeabsichtigtes Schließen des Spiels bei Rechtsklick auf Buttons z.B. in der Stadtansicht verhindert.
- Die zufällige Auswahl eines Rohstoffs beim Hinzufügen eines Spielers zu einem Spiel konnte dazu führen, dass die Ermittlung des verfügbaren Lagerraums fehlerhaft war und die gesamte Ware gespendet wurde.
- Die Talerbeträge von finanziellen Zufallsereignissen sind nun im Falle eines Gesamtvermögens des Spielers von 0 oder weniger (Schulden) nicht mehr 0 sondern entsprechend einem Betrag relativ zum Startkapital des Spielers.
- Zahlreiche Rechtschreibfehler ausgebessert.
- Ein Fehler beim Tod des letzten Spielers wurde behoben.

### Balancing:

- Brautwerbung: Geschenke und deren Auswirkungen neu ausbalanciert.
- Lagerraum: Lagerraumpreis verdoppelt und Startlagerraum von 20 auf 30 erhöht.
- Lagerraum: Toleranz von 4 Einheiten einer Ware bei mangelndem Lagerraum eingebaut, d.h. es werden immer mind. 5 Einheiten gespendet aber nicht weniger. Für 4 Einheiten findet sich immer irgendwo Platz ...

## 1.3.4 - 24.12.2017
Folgende Fehler wurden behoben:
- Bei Beendigung des Spiels mittels ALT + F4 wird nun auch die Musik beendet.
- Änderungen in den Einstellungen wurden erst nach Neustart wirksam.

Folgende Erweiterungen/Änderungen wurden implementiert:
- Die angezeigten Stadtinformationen wurden erweitert:
- Es werden nun alle produzierbaren Rohstoffe einer Stadt angezeigt.
  - Lagerbestand wird ebenfalls angezeigt.
  - Haupt- werden gegenüber Nebenproduktionen optisch hervorgehoben.
- Lagerraum funktioniert jetzt wie in Fugger 2
- Musikstücke und Soundeffekte wurden auf ein neues Format umgestellt.
- Zahlreiche sonstige kleinere Optimierungen, welche Conspiratio als gesamtes besser spielbar machen.
- Ein Geldklimpergeräusch wird nun bei jeder Abrechnung abgespielt.
- Die Mengeneingabe wurde überarbeitet

## 1.3.3
Folgende Fehler wurden behoben:
- Zahlreiche Rechtschreib- und Grammatikfehler wurden behoben
- Unbesetzte Ämter können nicht länger angewählt werden
- Beim Rückziehen einer Bewerbung wurde das Symbol nicht aktualisiert
- Das Privilegienfenster wird nur noch geschlossen, wenn ein Spieler sein Amt niederlegt
- In seltenen Fällen konnte ein Spieler bei einer Absetzung doppelt abstimmen
- Das Flimmern der Ladeform wurde entfernt 
- Der Tod eines menschlichen Spielers konnte für die nächste Generation einen Fehler bei der Wahlteilnahme auslösen
- Ein Fehler bei Prozessen wurde behoben
- Durch einen Trick konnten in einem Jahr beliebig viele Prozesse initiiert werden
- Privilegium Zollkartell: Die Zollkosten wurden bei der Abrechnung fälschlicherweise angezeigt

Folgende Erweiterungen/Änderungen wurden implementiert:
- Das Öffnen der Stadtinformationen wurde wesentlich beschleunigt
- Neue Tipps wurden hinzugefügt
- Am Beginn jedes Jahres wird nun ein zufälliger Tipp angezeigt
- Ein Optionsfenster wurde implementiert, welches dem Spieler das Einstellen von Musik und Tippsanzeige ermöglicht
- "Golddukaten" wurden in "Taler" umbenannt
- Die Fertigstellung eines Wohnsitzes wird nun am Rundenende angezeigt
- Gerichtsverfahren wurden erweitert
- Folgende Privilegien wurden implementiert:
  - Günstige Kredite (Freigeschalten als Vogt/Vögtin)
  - Zollfrei (Freigeschalten als Zöllner/Zöllnerin) 
  - Prediger (Freigeschalten als Priester/Priesterin)
- Speicherstände werden verschlüsselt abgespeichert
- Anwesen können erweitert bzw. ausgebaut werden
- Setup erneuert

## 1.3.2
Folgende Fehler wurden behoben:
- Die Beziehungen zwischen den einzelnen Computerspielern wurden nicht gespeichert.
  - Dieser Umstand führte dazu das nach dem Laden eines Speicherstandes viele Feindschaften entstanden wo früher keine waren und umgekehrt. Dies hatte sehr viele umsinnige Neuwahlen zur Folge, welche auf dem Spieler zur Last fallen konnten
- Conspiratio konnte nur gestartet werden, wenn der Benutzer ein Office-Paktet installiert hat
- Die Funktion Geldleiher in der Schreibstube konnte unter gewissen Umständen nicht verwendet werden
- Bei einem Spieler alleine wurde im Buch mit den Kontrahenten häufig eine letzte leere Seite angezeigt
- Sobald der letzte Spieler verstarb und keinen Erben hinterlies, öffnete sich automatisch ein neues Spiel mit einem namenlosen Spieler
- Unter seltenen Umständen konnte der Spieler seine Gesundheit über das Maximum hinaus erhöhen und dadurch seinen Todeszeitpunkt massiv hinauszögern
- Beim Exportieren wurden die Karavanenkosten irrtümlicherweise auch bei einem "Export" von 0 Waren eingehoben
- Die Bezahlung der Kupplerin wurde nicht sofort durchgeführt

Folgende Erweiterungen/Änderungen wurden implementiert:
- 18 neue Zufallsereignisse wurden implementiert, welche Einfluss auf Ansehen, Reichtum und Gesundheit nehmen können.
- Conspiratio Nachrichten (wie z.B. Abrechnung, Bestechen,...) öffnen nicht länger eigene Fenster, welche in der Windows Taskleiste angezeigt werden  
- Eine neue Schriftart wird verwendet
- Die Speicher- und Ladegeschwindigkeit wurde verbessert
- Es ist nun möglich die Wahlbewerbung zurückzuziehen
- Ältere Autosaves werden automatisch gelöscht
- Das Privilegium "Vergifteter Wein" verbilligt nun Mordanschläge

Balancings:
- Beim Werben um einen Partner wurden die Geschenkskosten um ~50% reduziert
- Das Werben um einen Partner wurde etwas beschleunigt
- Kreditzinsen von 25-35% pro Jahr auf 10-20% pro Jahr festgelegt
- Karavanenkosten um ~50% reduziert
- Preisuntergrenze der Startrohstoffe von 6 auf 7 erhöht
- Grundpreis der Werkstätten um 20% reduziert
- Die "Maklergebühr" für die Kupplerin wurde um 50% reduziert

Sound:
- Der Musikwechsel beim Betreten des Hinterzimmers wurde vorerst deaktiviert

Graphische Änderungen:
- Intro
- Gericht
- Nachrichten
- Kupplerin
- Münzsymbol für Wahlen
- Religionssymbole
- Kriminalitätssymbol

## 1.3.1
Folgende Fehler wurden behoben:
- In manchen Fällen wurden die Bestechungssummen erst am Ende des Zuges abgezogen und nicht sofort.
- Einige Grammatikfehler wurden behoben.
- Beim Laden eines Autosaves gingen manchmal Waren verloren.

Folgende Erweiterungen/Änderungen wurden implementiert:
- Bestechungen werden erst am Ende des Zuges ausgeführt (ist zugleich Balancing. Die Beziehung zu der KI ändert sich am Ende des gleichen Zuges, daher erfährt man nicht länger sofort, welchen genauen Einfluss das Bestechen hatte)
- Es gibt nun 10 verschiedene Todesursachen.
- Über 50 neue Zufallsereignisse wurden implementiert, welche Einfluss auf Ansehen, Reichtum und Gesundheit nehmen können.

Balancings:
- Brautwerbung: Teure Geschenke erobern das Herz der/des Angebeteten nicht mehr so schnell
- Brautwerbung: Geschenkepreise sind nun weniger vom eigenen Reichtum und mehr vom Reichtum der/des Umworbenen abhängig.

Sound:
- Wunderschöne Musikstücke wurden hinzugefügt um die Atmosphäre zu verbessern. Diese stammt von Jason Shaw. Besucht doch mal seine Homepage: http://audionautix.com/index.php ;-)

## 1.3
Folgende Fehler wurden behoben:
- Justizgesetze heißen ab nun überall Strafgesetze
- Das Amtseinkommen wurde immer überwiesen jedoch nicht immer angezeigt
- Waren in einer Runde >= 6 Ämter frei, so konnte man nur für die ersten
		5 freien Ämter kandidieren
- Bei den Bewerberinformationen zu einem Amt, stand manchmal fälschlicherweise
		"wird durch los entschieden"
- Nach Neustart und laden, schnappten sich hinterhältige KIs einfach die freien Ämter
- Durch geschicktes Laden und Speichern konnte ein Spieler an mehreren Wahlen teilnehmen
- Öffnen der Cheatbox mit negativem Kapital führte zum Absturz
- Brautwerbekosten wurden teilweise falsch angezeigt
- Mit genügend negativen Ansehen konnte man auch mit positiven Golddukaten im Schuldturm landen

Folgende Erweiterungen/Änderungen wurden implementiert:
- Unpassende KI-Namen, sowie Namen die Umlaute
		enthielten wurden ersetzt
- Medikus gibt nun Auskunft über Gesundheit und Alter
- Gläubiger entscheiden nun über einen Aufenthalt
		im Schuldturm
- Schreibstube: Kontrahenten können nun alle auf einen
		Blick abgerufen werden
- Neue Privilegien:
	- Kirchengesetze festlegen
	- Finanzgesetze festlegen
	- Strafgesetze festlegen
	- Steuerhinterziehung
- KI-Spieler ändern Gesetze
- KI-Spieler haben nicht immer die gleiche Startaufstellung
		(1600: gleiches Amt, gleiches Vermögen,...)
- Neue Gesetze:
	- Maximale Golddukaten
	- Glücksspiel
	- Goldabbau
	- Silberabbau
	- Waffenhandel
- Testmodus hinzugefügt. Dieser gibt bei manchen Aktionen genaue Einblicke
		in Berechnungen usw. Dieser wird noch erweitert
- Ämter werden nun gegendert
- Statistik am Ende des Spiels (Ideen für weitere Einträge sind stets
			willkommen)
- Die Anzahl der Niederlassungen sind nicht länger auf 3 beschränkt
- Spionageinformationen werden nun in Schreibstube/Kontrahenten aufgelistet
- zahlreiche neue Zufallsereignisse, welche sich auf Finanzen, Ansehen
		und Gesundheit auswirken können
- Im Hinterzimmer können Opfer nun auch über eine Liste ausgewählt werden. 
		Dieser Button befindet sich rechts oben auf der Weltkarte
- Rohstoffe neu verteilt
- Folgende Privilegien vorübergehend entfernt:
	-Händler
	-Kaufmann
	-Merchant
- Der Menüpunkt Tipps wurde entfernt
- Menüpunkt Optionen hinzugefügt
- KI-Teilnehmer pro Wahl von 3 auf 2 reduziert
- KI
- Rohstoffe geändert: 	Tabak --> Kupfer
				Tuch  --> Medizin

Balancings:
- Teure Wohnsitze(Schloss, Burg,...) erhöhen die
		Gesundheit während schäbige(Kate und Hütte)
		sie schmälern
- Brautwerbekosten erhöht, Werbedauer verkürzt
- Bestechen erschwert
- Produktionskosten von Rohstoffen überarbeitet
- Sabotagekosten variieren nun. Sie sind abhängig vom Gesamtvermögen der 
		Zielperson, sowie deren Amt
- Auch der angerichtete Schaden bei einer Sabotage hängt nun vom Gesamtvermögen
		des Spielers ab
- Bauzeiten der Anwesen verringert
- Das Innehaben mehrerer Anwesen wirkt sich nicht länger negativ auf die
		Gesundheit aus
- Produktionsverhältnisse von Ziegel und Rum verändert
- Neues Wirtschaftssystem implementiert, sodass Preise der Rohstoffe nun passender sind
- Stadtwache - Privilegium Korruptionsgelder abgeschwächt

Graphische Änderungen:
	
Sound:

## 1.2.7
Folgende Fehler wurden behoben:
- Beim speichern von Spielständen ging der bis dahin erzielte Umsatz
		von diesem Jahr verloren und daher musste dieses Jahr auch
		kein Kirchenzehnt oder Ähnliches bezahlt werden
- Transportkosten von Karawanen wurden falsch berechnet
- Windows Installer 4.5 wurde nicht automatisch mitinstalliert,
		daher konnten manche Benutzer Conspiratio nach der
		Installation nicht starten

Folgende Erweiterungen/Änderungen wurden implementiert:
- Neue Privilegien:
	- Hand des Henkers
	- Korruptionsgelder
	- Schmuggel
	- Zollkartell
- Viele neue Beschreibungen sind jetzt über Rechtsklick aufrufbar
		(wenn der Cursor mit dem Schwert ein Fragezeichen dabei hat)
- Wegzölle müssen nun entrichtet werden

Balancings:
- Handelszertifikate IDs:
	- Zertifikat #3: 5-15 -> 8-15
- Maximale Anzahl an Wohnsitzen auf 3 gesetzt
- Betriebe können nur noch in Städten eröffnet werden, in denen auch
		ein Wohnsitz vorhanden ist
- Karawanenkosten reduziert
- Einkommen von politischen Ämtern erhöht

Graphische Änderungen:
- Kontor: Hintergrund ausgetauscht

Sound:

## 1.2.6
Folgende Fehler wurden behoben:
- Brautwerbung: Ein Klick auf "Ist mir alles zu teuer auswählt" führte zum
		Kauf eines Randomgeschenks (Danke an Thomas H. Wichtel)
- Länderwahlen: Bei der Wahl von Ämtern auf der ländlichen Ebene wurden
		teils falsche Wähler herangezogen und hatte zur Folge, dass menschliche
		Spieler bei Wahlen zusahen, bei denen sie kein Stimmrecht besaßen
- Nach einer Amtsenthebung wurden die Totenköpfe nicht wieder durch das Fragezeichen
		ausgetauscht
- Wurde an die/den Gattin/Gatten vererbt, so bekam man den Titel von dieser/m
		(Dadurch konnte auch der Titel verschwinden)
- KIsKlagen: Falls eine KI eine Klage gegen einen menschlichen Spieler erhob,
		so konnte es in 1/10 Fällen zum Absturz führen
- Ein Fehler wurde behoben der bei der Wahl oder der Amtsenthebung der Stadtoberen Ämter
		(Bürgermeister, Domherr, Stadtkommandant) auftreten konnte
- Beim Speicher/Laden ging das Amt der menschlichen Spieler verloren und führte
		dementsprechend zu schweren Folgefehlern (Danke Sir Toby für den Hinweis)
- Das Laden eines Spielstandes führte früher oder später im laufenden Spiel
		zum Absturz
- Unter Umständen konnte sich Conspiratio beim Einblick in die Wahlinformationen
		aufhängen
- Permanenter Verkauf war gratis
- Permanenter Verkauf konnte Waren verschwinden lassen
- Exporte in der Ersten Runde wurden nicht durchgeführt
- Sehr selten war es möglich, dass man nach einer Wahl von seinen früheren Vorgesetzten
		des neuen Amtes enthoben worden ist
- Es war möglich mit negativer Bilanz Waren negativ einzukaufen (also zu verkaufen)
		und damit konnte man fast seine gesamten Schulden wegcheaten 
		(Danke Dorny für den Hinweis)
- Titel gingen beim Speichern verloren und konnten dannach auch nicht wieder
		erlangt werden

Folgende Erweiterungen/Änderungen wurden implementiert:
- Eine Hochzeit hebt nun die Titel beider Beteiligten auf dasselbe Level
- Beim Startup-Screen wird nun rechts unten ein Hinweis "Rechtsklick um fortzufahren"
		angezeigt (Danke für die Idee an B-K)
- Jedes Spiel hat nun einen Spielnamen. Im Ordner
		C:\Users\%Username%\AppData\Roaming\Conspiratio werden die gesamten
		Spielstände gespeichert. Jedes Jahr wird dort außerdem ein
		Autosave erstellt.
- Falls jemand beim Gericht schuldig gesprochen wird, so vollzieht das Gericht 
		nun Strafen (Pranger, Kerker oder Geldstrafen)
- Erpressung im Hinterzimmer wurde vorübergehend entfernt
- Königreich, Länder und Städte wurden umbenannt
- Credits (Verbesserungsvorschläge erwünscht)
- Werkstättenboni vorübergehend entfernt
- Werkstätten können nun auch verkauft werden

Balancings:
- Der Preis beim Kauf einer Werkstätte hängt nun vom Rohstoff ab

Graphische Änderungen:
- Kupplerin: Hintergrund wurde ausgetauscht
- Icon: Das verzierte C wurde ausgetauscht durch ein Siegel
- Banner ausgetauscht
- Cursor: Die Standardwindows-Cursor wurden durch selbstgemachte
		von unserem Grafiker ausgetauscht. Die neuen Cursor geben
		unter anderem mit zusätzlichen Symbolen (+, -, ?) auch 
		Hinweise auf Funktionen
- neue Landkarte

Sound:
- Sämtliche Sounds vorerst deaktiviert

## 1.2.5
Folgende Fehler wurden behoben:
- Kehrte man ins Hauptmenü zurück, so wurden die Spielerinformationen
		nicht ausgeblendet
- Titelverleih: Regent in Mittelland wurde nicht vollständig angezeigt
- Beim Tod übernimmt der Erbe nicht länger die Gesundheit des Verstorbenen
- Beim Zurückkehren ins Hauptmenü von den Optionen wurde das Spiel nicht neu
		initialisiert (d.h. Sympathien von KIs,... blieben erhalten vom vorigen Spiel)
- Stadtbeschreibung: Katastrophenbilder werden nicht länger angezeigt
- Neues Spiel: Rechtsklick schloss das Fenster und führte zum Absturz
- Cheatbox: Bei Amt übernehmen müssen nun zuerst alle Felder ausgewählt werden
		bevor der Button aktiviert werden kann
- Erpressung: Die Sympathie sank zwar, aber sie wurde nicht sofort aktualisiert
- Anschwärzen: Es war nur möglich Ebenenintern anzuschwärzen
- Hofräte waren vertauscht
- Rohstoffwechsel beim Export führt nicht länger zu Warenverlust
- Klick auf den Baum im Hausbau-Modus führte zum Absturz
- Beim Laden eines Spiels wurde die Einstellung Cheats(An/Aus) und
		Todesfälle(An/Aus) nicht geladen


Folgende Erweiterungen/Änderungen wurden implementiert:
- Versionsnummer wird nun ausgeblendet sobald das Spiel gestartet wird
- Neuen Spieler Hinzufügen: Bild wird wieder maximiert
- Sterbeformel wurde komplett neu überarbeitet. Bevorstehende Tode werden
		vom Medikus nun zu 100% erkannt und bekanntgegeben
- Feindliche Handlungen im Hinterzimmer können nun auch gegen menschliche
		Mitspieler durchgeführt werden
- Haus bauen: Das Bild, welches die Baustelle anzeigt, wird jetzt sofort
		umgestellt und nicht erst beim nächsten Betreten der Stadt
- Sabotage und Spionage wurde neu überarbeitet
- Sabotage- und Spionagekosten werden nun in der Abrechnung angezeigt
- Neue Privilegien:
	- Wache
	- Leibgarde
- Bei Klick auf Einzelspieler wird eine neue Form aufgerufen in der man zwischen
		"Laden" und "neues Spiel" entscheiden kann

Balancings:

Graphische Änderungen:
- Gerichtsicons neu ausgerichtet

Sound:
- Rechtsklick: Geräusch wurde eingebaut
- Nächster Spieler: Wird nun mit einem Hahnenkrähen angekündigt

## 1.2.4
Folgende Fehler wurden behoben:
- Permanent Verkaufen: Stadt und Rohstoff konnten nicht eingestellt werden
- Todesfälle anzeigen: Manchmal wurde nach den Todesfällen noch eine weitere
		leere Seite angezeigt

Folgende Erweiterungen/Änderungen wurden implementiert:
- Neues Spiel: Neue Feineinstellungen wurden hinzugefügt.
- Neuer Spieler hinzufügen: Besitzt nun eine eigene Form
- Katastrophen wurden vorerst entfernt
- Falls der Ehepartner eines menschlichen Spielers stirbt, so
		wird dies auch angezeigt

Balancings:

Graphische Änderungen:

## 1.2.3
Folgende Fehler wurden behoben:
- Die Umsätze wurden nicht stadtbezogen sondern rohstoffbezogen gespeichert.
		Dies führte beim späteren Aufruf von bestimmten Rohstoffen zum Absturz
- Erzdiakon und Inquisitor waren auf der Ämterebene vertauscht
- Starb ein menschlicher Spieler ohne Amt, so führte dies zum Absturz
- Nach dem Tod eines menschlichen Spielers wurde manchmal der Hintergrund 
		nicht sofort geändert
- Goldanzeige beim Auftraggeben eines Mordes wird nun sofort aktualisiert
- Goldanzeige beim Ablasskauf wird nun sofort aktualisiert
- Es wurde nicht richtig angezeigt ob Bestechungen erlaubt oder verboten waren
- Bauwerke stiften: Hospital führte zum Absturz
- Bauwerke stiften: Goldanzeige wird nun sofort aktualisiert
- Abt und Diakon waren auf der Ämterebene vertauscht
- Nach dem eigenen Tod wird der Name nun sofort aktualisiert
- Nach einem Einblick in das Kreditbuch, änderte der Cursor in den Ladekreis
		(was auch so gehört). Allerdings wurde diese Änderung nach dem
		Verlassen des Kreditbuches nicht rückgängig gemacht

Folgende Erweiterungen/Änderungen wurden implementiert:	
- Einen Heben gehn und Beleidigen wurde vorerst deaktiviert
- Startup-Sound entfernt
- Speicherplatzbedarf von Conspiratio erneut verringert
- Kirche: Hintergrundbild wird beim Konvertieren nun sofort gewechselt
- Stadtexport: Neue Funktion "Permanenter Verkauf" hinzugefügt. Damit
		wird jede Runde automatisch der Export auf die maximale Anzahl gesetzt
- Anforderungen an die Leistung des PCs reduziert. Handel: Die Banner
		sollten nun schneller ihre Farben annehmen
- Buch: Produktions- und Exportanzeige überarbeitet
- Privilegien:
	- Vergifteter Wein - nur Kellermeister
- Die Stadtinformationen werden nicht länger neben der Landkarte angezeigt.
		Mittels Rechtsklick auf eine Stadt können nun die Stadtinformationen
		ähnlich wie bei Fugger 2 aufgerufen werden.
- Einige unpassende KI-Namen wurden ausgetauscht

Balancings:
- Ansehenboni von Häusern verringert
- Produktionskosten für Rohstoffe neu balanciert
- Ermordung: Erfolgschance von 100% auf 33% reduziert

Graphische Änderungen:
- Einige Controls beim Kartenspielen wurden neu ausgerichtet
- Das Flackern des Testaments wurde behoben
- Optionen: Button-Hintergründe passender gestaltet
- Schreibstube: Hintergrundbild wurde ausgetauscht
- Schreibstube: Bewerbungen wird nur angezeigt, wenn auch Ämter frei sind
- Kindstod: Hintergrundbild wurde ausgetauscht
- Gericht: Hintergrundbild wurde ausgetauscht
- Sämtliche Rohstoffgrafiken wurden ausgetauscht
- Nachwuchs: Hintergrundbild wurde ausgetauscht

## 1.2.2
Folgende Fehler wurden behoben:
- Bei der Amtsenthebung blieb manchmal ein Teil vom Text permanent sichtbar
- Bestimmte Rohstoffe konnten in der Produktion nicht eingestellt werden
- Beim Testament hat ein böser Copy-Paste-Fehler eingeschlichen, welcher
		beim Ändern des Testaments zu einem Absturz führte
- Ein sehr seltener Fehler, der nur beim Innehaben des Amtes Kerkermeister
		auftreten konnte, wurde behoben
- Beim Exportieren waren keine Feineinstellungen beim unteren Produktionsslot
		möglich

Folgende Erweiterungen/Änderungen wurden implementiert:	
- In jeder Stadt von links nach rechts geordnet ist jeder zweite Rohstoff
		nun Hauptproduktion
- Folgende Privilegien wurden hinzugefügt:
	- Bauwerke stiften
	- Händler
	- Kaufmann
	- Merchant
	- Umsatzsteuer festlegen
	- Sparplan
- Brautwerbung: Hochzeitsglocken wurde in den Kirchgang verschoben
 		und in Brautschau umbenannt

Balancings:
- Die Werkstättenpreise wurden komplett überarbeitet und sollten
		damit einen leichteren Start gewähren
- Karawanenkosten überarbeitet
- Das neue Ansehenssystem wurde wieder abgeschafft

Graphische Änderungen:
- Folgende Grafiken wurden durch selbstgemachte von Beetle ersetzt:
	- Kontor
	- Stadthintergrund
	- Hinterzimmer
	- Kirche
	- Buch
	- Spielerankündigung
	- Kirchgang
	- Hauptmenü
	- Grab (Spieler stirbt)
	- Wahlhintergrund
	- Intro/Ending
- Cursor ändert sich in ein "Wartesymbol", falls ein Rechtsklick gefordert
		wird um das Spiel fortzusetzen

## 1.2.1
Folgende Fehler wurden behoben:
- Bei den Wahlen von den höchsten Ämtern einer Stadt oder 
		eines Lands wurde bei den Informationen angezeigt, dass diese
		durch Losentscheid fallen würde. Nun werden die richtigen Wähler
		angezeigt
- Bestechen: In manchen Fällen wurde das 10-fache vom angegebenen Betrag
		gezahlt
- Wahl: Die abgegebene Stimme von einem Wähler, wurde zwar dem richtigen
		Kandidaten zugeordnet, die Anzeige aber sagte manchmal, dass ein 
		anderer Kandidat die Stimme bekam
- Beim Eingeben eines Namens und dem anschließendem Enterdruck ertönt
		nicht länger ein Windows-Fehlersound
- Wurde ein Spielstand geladen, so wurde das Alter aller menschlichen Spieler
		zurück auf 20 gesetzt
- Ein Speicher/Lade-Fehler wurde behoben, welcher dazu führte, das sämtliche
		Daten von den menschlichen Spielern (außer dem ersten) verloren gingen
- Erbschleicher können nicht länger das Spiel übernehmen
- Zwei schwere Fehler, die zum Absturz führten, wurden behoben
- In seltenen Fällen wurde wenn man im Optionsmenü auf "Zurück zum Spiel"
		klickte das Spiel vollständig beendet
- Besaß man unverheiratet Kinder, so konnte kein Testament geschrieben werden
- Unter Umständen wurde das Bild von den getauschten Ringen bei einer Hochzeit
		erst verzögert wieder ausgetauscht
- Das Einsehen in die Gesetze verursachte einen Absturz
- Ein zweimaliges konvertieren des Glaubens führte zum Absturz
- Es war möglich eine Person bei sich selbst anzuschwärzen
- Privilegium Handelszertifikate wurde nie angezeigt

Folgende Erweiterungen/Änderungen wurden implementiert:	
- Wahl: Falls ein Spieler dran ist, beim Wählen seine Stimme abzugeben,
		so wird dieser mittels Name dazu auch aufgefordert (Dies konnte
		bei 2 oder mehr Spieler schnell für Verwirrung sorgen, da nicht
		bekannt war, welcher Spieler nun zum Wählen dran war)
- Bei der Geburt eines Kindes wird nun auch dessen Geschlecht angezeigt
- Die Cheatbox bietet nun neue Möglichkeiten und Hilfestellungen für das 
		Testen
- Testament gegendert nun
- Auch Produktionseinstellungen werden ab nun gespeichert und geladen
- Kirchenzehnt und Verkaufssteuern kommen als zusätzliche finanzielle 
		Belastung für den Spieler hinzu
- Spielernamen müssen ab nun aus Mindestens 3 Buchstaben bestehen
- Es können nicht länger 2 menschliche Spieler den selben Namen wählen
- Nicht benützte Grafiken, Sounds, Codesegmente wurden entfernt. Der benötigte
		Speicherplatz wurde damit um die Hälfte reduziert
- Ein primitives Rechtssystem wurde implementiert. Anders als bei Fugger 2 wird
		man hier nur für jene Taten belangt, die man auch begangen hat

Balancings:
- Der Medikus ist nun verlässlicher
- Sämtliche Rohstoffspreise wurden erhöht
- Brautwerbekosten wurden deutlich verringert

Graphische Änderungen:
- Das Bild bei der Geburt eines Kindes wurde ausgetauscht
- Beim Kreditetilgen wurden einige Verschiebungen angepasst

## 1.2.0
Folgende Fehler wurden behoben:
- Bei den Golddukaten wurde unter Umständen ein Trennzeichen 
                falsch gesetzt
- Das Amtseinkommen wurde stets angekündigt, aber es ist
		nie "überwiesen" worden
- Das Privilegium "Amt niederlegen" funktionierte nicht.
- In der Cheatbox führte das Betätigen der Checkbox "alle"
		zum Absturz
- Unter bestimmten Bildschirmauflösungen war das Einstellen der Arbeiter
		und der Werkstätten deutlich verschoben
- Es war möglich auch mit positivem Vermögen in den Schuldturm zu landen
- Bei 2 oder mehr Spielern konnte nur der letzte Spieler die
		Funktion "Ermordung" nutzen
- In seltenen Fällen war es möglich einen zusätzlichen Rohstoff
		zu produzieren ohne überhaupt eine Werkstätte von jenem zu besitzen
- Ein häufiger kritischer Fehler wurde behoben, bei dem sich das ganze
		Programm aufgehängt hat.
- Beim Exportieren wurde nur der stadtinterne Verkaufspreis angerechnet
- Ein Fehler bei der Darstellung von den Priestern wurde behoben
- Ämter sind nicht länger kongenital

Folgende Erweiterungen/Änderungen wurden implementiert:
- Im Hauptmenü können nun auch einige Tipps zu Conspiratio 
                eingesehen werden
- Religionszugehörigkeit wirkt sich nun auf Wahlen aus
- Das Konvertieren zu einem anderem Glauben ist nun möglich
- Das Austreten von der Kirche ist nun auch möglich
- Die Religionszugehörigkeit von Computergegnern wird nun in der
		Ämterhierarchie angezeigt
- Spielernamen sind nun auf eine maximale Länge von 12 Zeichen beschränkt
- Spielernamen können auch keine Leerzeichen mehr enthalten
- Hausbau: Ein Klick auf die Baustelle zeigt an, wann das Gebäude 
		fertiggestellt wird

Balancings:
- Computergegner haben bei Wahlen nun einen kleinen Vorteil
- Rohstoffproduktion etwas erhöht
- Produktionskosten abhängig von der Wertigkeit des Rohstoffes erhöht
- Neuen Spieler Hinzufügen: 
  - Die Kosten für die Wahl der Stadt wurden auf 150 Golddukaten reduziert.
  - Die Kosten für die Wahl des Rohstoffes wurden auf 100 Golddukaten gesetzt

Graphische Änderungen:
- Schuldturm/Kerker (Bild) wurde ausgetauscht

## 1.1.28
Folgende Fehler wurden behoben:
- Unter gewissen Umständen konnte man einen Rohstoff in einer Stadt nicht
		herstellen obwohl sämtliche Bedingungen dafür erfüllt waren
- Der Spieler konnte nicht mehr als 5 Privilegien inne haben
- Im Falle des Todes und einer Wahlteilnahme in derselben Runde übernahm
		der Erbe automatisch den Platz bei der Wahl des Toten

Folgende Erweiterungen/Änderungen wurden implementiert:
- Handelszertifikate werden nun von den zuständigen Stellen verliehen
- Die Zugehörigkeit zu einer Religion wirkt sich auf die Sympathie aus
- Auch Computergegner besitzen nun Titel
- Der Cheat "Beetle" wurde deaktiviert.
- Die "Cheatbox" wurde implementiert, welche sich durch 
                  das Drücken von "u" öffnen lässt
- Privilegien:
	- Kerkerklatsch

Balancings:
- Spionagekosten um den Faktor 10 verringert.
- Sabotagekosten um den Faktor 10 verringert.

Graphische Änderungen:
- Kupplerin (Bild) wurde ausgetauscht
	
## 1.1.27
Folgende Fehler wurden behoben:
- Die Besitzer von Ämtern aus höheren Ebenen wurden nicht angezeigt
- Unter gewissen Umständen wurde ein freies Amt angezeigt, wofür man
		sich nicht bewerben konnte
- Es konnten sich nicht 2 Menschliche Spieler für das gleiche Amt bewerben
- Beim Bestechen entsprechen die Beträge nun wieder der angeklickten Potenz
		(Es ist nicht länger versetzt)
- Manchmal wurde bei der Frage "Runde beenden", trotz Rechtsklick die Runde
		irrtümlich beendet
- Der Preis von Brautwerbegeschenken stimmte teilweise nicht

Folgende Erweiterungen/Änderungen wurden implementiert:
- Folgende Ämter wurden geändert
	- Kavallarist 2->	Zollmeister
	- Kavallarist 1->	Befehlshaber
	- Infantrist 1	->	Stv. Befehlshaber
	- Infantrist 2	->	Zöllner 1
	- Infantrist 3	->	Zöllner 2
- Folgende Privilegien wurden hinzugefügt:
	- Einkommen
	- Amtsenthebung (Auch Computergegner nutzen dieses Privilegium)
- Banner werden nun auch im Hinterzimmer angezeigt
- Rohstoffpreise schwanken nun leicht
- Beziehungen zu KIs schwanken nun leicht
- Sämtliche Todesfälle von Konkurrenten werden nun angezeigt

Balancings:
- Sterbechancen geändert

Graphische Änderungen:
- Pergamentspapier wurde durch eines von unserem Grafiker ersetzt

## 1.1.24
Folgende Fehler wurden behoben:
- Personen aus höheren Ebenen, die getötet werden, 
		behalten nicht länger ihr Amt bei
- Bei Bestechungen des Geheimrat 1, gingen diese an Geheimrat 3. Die
		von Geheimrat 3 and Geheimrat 2 und die von Geheimrat 2 and
		Geheimrat 1.1. Selbiger Fehler trat auch bei den Hofräten auf
- Zu Wahlen aus höheren Ebenen konnte nicht angetreten werden

Folgende Erweiterungen/Änderungen wurden implementiert:
- Grafschaftliche Ebene wurde vorübergehend entfernt
- Folgende Ämter werden nun gewählt durch:
	- Bürgermeister -> 	Vogt
	- Domherr	->	Bischof
	- Stadtkommandant->	Hauptmann
	- Vogt		->	Regent
	- Bischof	->	Erzbischof
	- Hauptmann	->	Feldmarschall
- Folgende Ämter wurden geändert
	- Marschall 	-> 	Feldmarschall
	- Armeekomandant -> 	Marschall
	- Ritter 	-> 	Offizier
	- Wachkommandant 2->	Kerkermeister
	- Stadtwache 2	->	Folterknecht
	- Stadtwache 3	->	Henker
- Titel eingeführt

Balancings:
- Fürstenbrück wurde zum Land Bergenhöh hinzugefügt
- Mindestanforderung um ein Amt in einer Stadt zu bekleiden:
	- 1 Haus in dieser Stadt
	- Titel Bürger oder höher
- Mindestanforderung um ein Amt in einem Land zu bekleiden:
	- 2 Häuser in diesem Land
	- Titel Ritter oder höher
- Mindestanforderung um ein Amt im Reich zu bekleiden:
	- Titel Freiherr oder höher

## 1.1.23
Folgende Fehler wurden behoben:
- Das Testament hält den Erben nicht länger vom Sterben ab
	
Folgende Erweiterungen/Änderungen wurden implementiert:
- Ansehen wurde implementiert
- Warnung erscheint, falls für ein Vorhaben nicht genügend Golddukaten
		vorhanden sind
- Schuldturm in Form von Kerker

Balancings:
- KIs besitzen entsprechend ihren Ansehen und ihrem Amt deutlich mehr Golddukaten

## 1.1.22
Folgende Fehler wurden behoben:
- Fehler bei Spionage/Sabotage/...
- Karawane konnte nicht geöffnet werden
- Es können nicht länger mehr als 99 Arbeiter beschäftigt werden

Folgende Erweiterungen/Änderungen wurden implementiert:
- Tausendertrennzeichen beim Kontostand
- Werbegeschenke kosten nun auch Golddukaten 
- Speicherdauer wurde von ~30 Sekunden auf deutlich weniger reduziert
- Kinder können nun versterben
- Mehrere Spielstände sind nun möglich

Balancings:

## 1.1.21
Folgende Fehler wurden behoben:
- Laden war nicht freigeschaltet

## 1.1.20
Folgende Fehler wurden behoben:
- Kartenspielen ist nicht länger mit weniger als 0 Golddukaten möglich
- Werkstättenkäufe führen nicht länger zu Abstürzen

Folgende Erweiterungen/Änderungen wurden implementiert:
- Kartenspielen: computer sagen nur mehr zu wenn der Spieler auch eine
		Mindestzahl an Golddukaten besitzt
- Rohstoffe können nun immer eingekauft werden. Die Sinnhaftigkeit davon
		wird sich im Laufe der Zeit herausstellen...
- Dynamische Grafikanpassung wurde verbessert
- Benutzerfreundlichkeit weiter verbessert (Rechtsklick funktionierte
		manchmal nicht)
- Der Rohstoff Ziegel wird nicht länger abwechselnd mit "Stein" oder "Ziegel
		bezeichnet
- Ermordung wurde implementiert
- Selbstständige Partnersuche bei Heirat implementiert
- Speichern
- Laden

Balancings:
- Werbedauer um Ehepartner erhöht

## 1.1.19
Folgende Fehler wurden behoben:
- Handelszertifikate konnten unter bestimmten Bedingungen nicht
		erhalten werden (nun sind bis zu vier möglich)
- Spieler kann nicht länger mehrere Ämter besetzen
- Spionagekosten sind nun abhängig vom Amt und Reichtum der Person
- Buchraster bei Transport wurde oft auch falsch angezeigt
- Bei Klicks auf bestimmte Stellen wurden die Aktionen nicht durchgeführt

Folgende Erweiterungen wurden implementiert:
- Anstatt "17 &amp 4" steht nun "17 und 4"
- Falls etwas noch nicht implementiert wurde, wird dies nun bei versuchtem
		Durchführen auch angezeigt
- Benutzerfreundlichkeit wurde verbessert

Balancings:
- Rohstoffproduktion erneut um die Hälfte reduziert

## 1.1.18
Folgende Fehler wurden behoben:
- Durch einen Trick konnten x-beliebig viele Arbeiter eingestellt werden
- Weit weniger Waren
- Es konnten mehrere Frauen geheiratet werden
- Öffnen des Testaments führte zum Absturz
- Gewann ein Computergegner eine Loswahl, so wurde nur seine ID angezeigt
- Ein computergegner konnte mehrere Ämter inne haben
- Man kann auch nicht mehr mit Glück unendlich alt werden
- Eine Ämterebene, in der ein menschlicher Spieler ein Amt hatte, konnte
		nicht angezeigt werden

Folgende Erweiterungen wurden implementiert:
- Bei der Produktion kann durch Rechtsklick auf den Rohstoff das verhältnis
		angezeigt werden
- Die Stadt in die exportiert wird, wird nun gespeichert
- Dynamische Grafikanpassung verbessert
- Cheat: Wer bei sich "Beetle" nennt bekommt 999999999 Goldukaten 
- Bis zu 4 Handelszertifikate sind nun erhältlich (Rohstoff ist Zufall)

Balancings:
- Anzahl der produzierten Waren auf die Hälfte reduziert
- Werbung um Frauen wurde "verlängert"
- Man bekommt weniger Kinder

## 1.1.12
Folgende Fehler wurden behoben:
- Absturz spätestens im Jahr 1618
- Absturz beim Bestechungsversuch
- Kinder konnten nicht benannt werden
