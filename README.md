# Conspiratio.Lib

Dies ist der aktuelle Stand der C# .NET Standard 2.0 Klassenbibliothek mit der Gameplay Logik von Conspiratio, entnommen aus dem [Conspiratio WinForms Client](https://github.com/Conspiratio/Conspiratio.WinForms). Die Bibiliothek ist noch nicht vollständig, enthält aber bereits die wichtigsten Klassen und Methoden und dient als Grundbaustein für den [Godot Client](https://github.com/Conspiratio/Conspiratio.Godot) dienen.

## Package

[![Nuget](https://img.shields.io/nuget/v/Conspiratio.Lib)](https://www.nuget.org/packages/Conspiratio.Lib/) [![GitHub release (latest SemVer)](https://img.shields.io/github/v/release/Conspiratio/Conspiratio.Lib)](https://github.com/Conspiratio/Conspiratio.Lib/releases) 

## Build

[![Push - Build and publish Lib](https://github.com/Conspiratio/Conspiratio.Lib/workflows/Push%20-%20Build%20and%20publish%20Lib/badge.svg)](https://github.com/Conspiratio/Conspiratio.Lib/actions?query=workflow%3A%22Push+-+Build+and+publish+Lib%22)  
[![Pull-request - Build Lib](https://github.com/Conspiratio/Conspiratio.Lib/workflows/Pull-request%20-%20Build%20Lib/badge.svg)](https://github.com/Conspiratio/Conspiratio.Lib/actions?query=workflow%3A%22Pull-request+-+Build+Lib%22)  
[![CodeQL](https://github.com/Conspiratio/Conspiratio.Lib/workflows/CodeQL/badge.svg)](https://github.com/Conspiratio/Conspiratio.Lib/actions?query=workflow%3ACodeQL)

Das Projekt wurde erstellt mit: Visual Studio 2019

Für den manuellen Build einfach die Projektmappe `Conspiratio.Lib.sln` öffnen und kompilieren.

## Systemvoraussetzungen / Abhängigkeiten
- .NET Standard 2.0 (keine Abhängigkeiten)

# Über das Spiel Conspiratio

Das Fanprojekt namens "Conspiratio" ist eine freie Wirtschaftssimulation der Neuzeit, die sich stark am Kultspielt "Die Fugger 2" orientiert.

Zu Beginn erbt der Spieler eine heruntergekommene Produktionsstätte und das bescheidene Ersparte eines Verwandten. Damit kann er sein Geschick als Kaufmann unter Beweis stellen, indem er Waren herstellt und verkauft, wohl durchdachte Investionen tätigt oder sich als gewiefter Exporteur durchsetzt. Der Spieler kann den neu gewonnenen Reichtum und den damit verbundenen Einfluss nutzen um:

- Noch weitere Produktionsstätten zu erwerben,
- Titel und Privilegien zu erlangen,
- Spione und Saboteure auszusenden
- Angesehene Amtsinhaber zu manipulieren,
- oder sogar selbst ein mächtiger Amtsträger zu werden.

Doch Vorsicht! Auch manche Konkurrenten werden von niederträchtigen Maßnahmen nicht zurückschrecken ...

# Über dieses Repository

Ziel ist ein Rewrite der Oberfläche und eine Portierung sowie Refaktorisierung der Gameplaylogiken und der gesamten Architektur von der aktuellen Windows Forms Version zu einem Godot Spiel, da wir hier viel mehr multimediale und vor allem grafische Möglichkeiten haben und es eine gewisse Plattformunabhängigkeit gibt. Dieser neue [Godot Client](https://github.com/Conspiratio/Conspiratio.Godot) wird vollständig Open-Source sein, wir möchten andere Menschen möglich einfach in die Mitarbeit und Mitentwicklung einbeziehen und aus dem Hobbyprojekt soll ein Communityprojekt, von Fans für Fans, werden.

Zur Planung und Steuerung der Entwicklung sollen Github Issues dienen.

# Mitmachen

Ihr wollt Euch an diesem Projekt beteiligen? Großartig! Tretet einfach mit uns über [Discord](https://discord.gg/dxkC5DPgRY) oder oldschool per <a href="&#109;&#97;&#105;&#108;&#116;&#111;&#58;%6D%61%69%6C%40%63%6F%6E%73%70%69%72%61%74%69%6F%2E%6E%65%74">E-Mail</a> in Kontakt und wir klären die Details.  
_Jegliche Hilfe ist willkommen._

## Git Workflow

**Wichtig: Wir committen und pushen nie direkt in den master Branch!**  
Der Grund ist einfach mangelnde Transparenz und fehlendes 4-Augen-Prinzip bzw. fehlende Kontrolle durch mind. einen anderen Entwickler.

Für jede Änderung an Conspiratio muss daher immer ein neuer, persönlicher Branch erstellt werden. Der Name des Branches sollte immer mit einem der folgenden Namen beginnen, gefolgt von einem Schrägstrich:
- improvement (= Verbesserung des Code oder einer Funktion im Spiel, auch Refaktorisierungen)
- fix (= Korrektur)
- feature (= neue Funktion des Spiels)

_Beispiel:_ fix/absturz-bei-ueberfall

Es sollten außerdem Umlaute und Sonderzeichen vermieden werden und es können außerdem aufgrund von technischen Restriktionen im Branchnamen Leerzeichen nicht verwendet werden, weshalb wir hier stattdessen Bindestriche verwenden.

Ist der eigene Branch dann soweit stabil und enthält alle gewünschten Änderungen/Erweiterungen, dann kann  mittels Pull Request eine Anfrage auf den Merge in den master Branch erstellt werden. Diese sollte immer einem anderen Entwickler zur Prüfung zugewiesen werden, welche einen kleinen Code Review macht, ggf. Feedback zum Code gibt und nach Ausbesserung den Branch dann auch mergt. Eigene Branches sollten nur in Ausnahmefällen selbst gemergt werden (z.B. zeitliche Dringlichkeit).

## Code Guidelines

Als Coding-Richtlinien für C# nutzen wir insbesondere für neuen Code folgende Referenz, da sich diese mittlerweile als Standard durchgesetzt hat:  
https://docs.microsoft.com/de-de/dotnet/csharp/programming-guide/inside-a-program/coding-conventions

Bezüglich der Benennung und der Standards wird zusätzlich noch diese Referenz herangezogen:  
https://www.dofactory.com/reference/csharp-coding-standards

Dabei ist bitte zu beachten, dass wir hier als Sprache der Kommentare im Code und auch der meisten Bezeichner deutsch verwenden, da die gesamte bestehende Codebase schon deutsch aufgebaut ist. Natürlich muss jetzt nicht jedes Keyword in jeder Methode komplett deutsch sein, z.B. wäre `GetUmsatzProSpieler` vollkommen legitim (da Get einfach für jeden Entwickler Standard sein sollte), problematisch wäre allerdings etwas wie `GetVolumeOfSalesPerPlayer`, da wir solche Begriffe sonst nirgendwo finden, weder in der Spieloberfläche noch im bestehenden Code und es daher schnell Verwirrungen geben kann, was nun gemeint ist.

Alter Code kann und sollte gerne nach und nach auf diese Richtlinien umgestellt werden, damit es später kein Durcheinander gibt, das hat aber zunächst mal nicht die höchste Priorität. Sollte man aber älteren Code verändern oder refaktorisieren, dann sollte man sich die Mühe machen, und hier auch die neuen Guidelines anwenden, frei nach dem Pfadfindermotto:  
_Hinterlasse einen Ort (Code) immer in einem besseren Zustand als du ihn vorgefunden hast._

## Dokumentation

Die Dokumentation von umfangreichen Features oder sonstigen interessanten Methoden, Klassen etc. im Code erfolgt im Github Wiki. Das Github Wiki soll ausschließlich der technischen Dokuementation und nicht der Dokumentation für die Spieler dienen, dafür wird es ein eigenes Wiki geben.

## Changelog

Vorab: Wir nutzen einiges aus diesem Konzept hier: https://keepachangelog.com/de/1.0.0/

Der Changelog wird in der Datei CHANGELOG.md gepflegt, direkt hier im Root. Wichtig ist, dass jede Änderung hier dokumentiert wird, und zwar immer im Bereich "Unreleased". Das bedeutet im Umkehrschluss, dass jeder Pull Request also auch immer eine Änderungen an der Changelog-Datei enthalten muss, sonst ist er nicht vollständig.

Im Changelog nutzen wir folgende Gruppen zur Unterteilung der Änderungen:

- Erweiterungen
- Änderungen
- Korrekturen
- Balancing