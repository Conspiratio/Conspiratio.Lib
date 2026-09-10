using System;

using Conspiratio.Lib.Gameplay.Kampf;
using Conspiratio.Lib.Gameplay.Kampf.Einheiten;
using Conspiratio.Lib.Gameplay.Niederlassung;
using Conspiratio.Lib.Gameplay.Spielwelt;

using JetBrains.Annotations;

namespace Conspiratio.Lib.Allgemein
{
    /// <summary>
    /// Berechnet und verbucht die Jahresabrechnung des aktiven Spielers beim Zugende.
    /// Extrahiert aus dem alten WinForms-Dialog "Abrechnung".
    /// </summary>
    public class AbrechnungsManager
    {
        /// <summary>
        /// Unterhalt der ersten besessenen Werkstatt; die n-te kostet das n-Fache.
        ///
        /// <b>Bemessungsgrundlage ist der Besitz</b>, gezaehlt ueber <c>HumSpieler.ZaehleWerkstaetten</c>
        /// - nicht die Produktionsplaetze, ueber die weiter oben die Auslastungsbelohnung laeuft. Die
        /// beiden Groessen werden in dieser Datei bewusst auseinandergehalten: <i>besessene Werkstatt</i>
        /// meint immer den Unterhalt, <i>Produktionsplatz</i> immer die Belohnung. Sie sind verschieden
        /// gross - der Besitz reicht bis 84, die Produktionsplaetze bis 28.
        ///
        /// Nach unten: Zwei Werkstaetten kosten drei Grundeinheiten, also 300 Taler - Rauschen gemessen
        /// am Startkapital, und mit Abstand unter den 500 Talern, die der Einsteigertest zulaesst.
        ///
        /// Nach oben: der Deckungsbeitrag einer ausgelasteten Werkstatt. Aus
        /// <see cref="Rohstoffe.Rohstoff.GetWSProdProWS"/> (90 % davon bei richtiger Arbeiterzahl) gegen
        /// <see cref="Rohstoffe.Rohstoff.GetWSEinzelpreis"/> und
        /// <see cref="Rohstoffe.Rohstoff.GetWSArbeiterpreis"/> traegt die schwaechste Ware <b>mit
        /// Arbeitergrenze</b> in einem bis zum Arbeiterdeckel besetzten Produktionsplatz 8 474 Taler im
        /// Jahr, die staerkste 20 438. Die Einschraenkung ist noetig: Wolle, Fell und Rum brauchen
        /// 1 Arbeiter je 2 Werkstaetten, ganzzahlig also 0, und haben damit ueberhaupt keine
        /// Arbeitergrenze - sie stehen ausserhalb dieser Spanne.
        ///
        /// <b>Der Betriebsbereich reicht weit ueber die kalibrierte Mitte hinaus.</b> Ueber die volle
        /// Spanne des Besitzes:
        ///
        /// <list type="bullet">
        /// <item>2 Werkstaetten: 300 Taler im Jahr - der Einsteigerfall.</item>
        /// <item>20 Werkstaetten: 21 000 Taler; die 20. allein 2 000.</item>
        /// <item>28 Werkstaetten: 40 600 Taler; die 28. allein 2 800 und damit ein Drittel des
        /// schwaechsten und ein Siebtel des staerksten Deckungsbeitrags.</item>
        /// <item>84 Werkstaetten - 14 Staedte mal <c>GetMaxWerkstaettenProStadt</c> (6), mit dem
        /// Anwesengesetz auf seiner Obergrenze (14) legal erreichbar: <b>357 000 Taler</b>; die 84.
        /// allein 8 400, also so viel, wie die schwaechste Ware mit Arbeitergrenze in einem vollen
        /// Produktionsplatz ueberhaupt einbringt.</item>
        /// </list>
        ///
        /// Am oberen Ende ist der Unterhalt damit keine Randnotiz mehr, sondern die Bremse selbst: Wer
        /// alles besitzt, was das Gesetz zulaesst, muss auch alles auslasten. Ob dieser Endausschlag so
        /// gewollt ist, ist eine Balancingfrage und hier ausdruecklich offen - die Kalibrierung des
        /// Wertes 100 zielte auf die Mitte der Spanne, nicht auf ihr Ende.
        /// </summary>
        public const int GrundunterhaltProWerkstatt = 100;

        /// <summary>
        /// Der Kapazitätsunterhalt in <b>Promille</b> je Produktionsstätte: Die n-te Stätte des Betriebs
        /// kostet das n-Fache eines Tausendstels dieses Werts im Jahr. Bei 646 Stätten und 140 sind das
        /// 29 257 Taler.
        ///
        /// <b>Warum es ihn braucht.</b> <see cref="GrundunterhaltProWerkstatt"/> ist zwar progressiv,
        /// bemisst sich aber am <i>Besitz an Werkstätten</i> - und daran wächst ein Händler nicht.
        /// Gemessen mit der Balancing-Harness: 14 Werkstätten, aber 646 Produktionsstätten und
        /// 1 360 Arbeiter, Unterhalt 10 500 Taler. Gewachsen wird <i>innerhalb</i> der Werkstatt, und
        /// Stätten wie Arbeiter kosten linear. Erlös linear, Kosten linear, Marge konstant: Das Vermögen
        /// stieg 40 Jahre lang geradeaus um rund 50 000 im Jahr, ohne je abzuflachen.
        ///
        /// <b>Die Spannung zum Kommentar an <see cref="AbrechnungsErgebnis.Unterhalt"/> ist gewollt und
        /// aufgelöst.</b> Dort steht, der Unterhalt hänge bewusst nicht an der Produktion, sonst wäre
        /// Horten gratis und Wirtschaften teuer. Das gilt weiter - der Werkstattunterhalt bleibt
        /// unverändert am Besitz. Wer zwanzig Werkstätten hält und in keiner produziert, zahlt sie
        /// voll. Der Kapazitätsunterhalt tritt daneben und trifft nur, was tatsächlich betrieben wird.
        ///
        /// Quadratisch über den <b>ganzen Betrieb</b> gerechnet, nicht je Linie: Je Linie war die
        /// Dreieckszahl viel zu steil (46 Stätten je Linie zu quadrieren trifft härter als 14
        /// Werkstätten) - schon 2 Taler Grundeinheit drückten das gemessene Endvermögen von 1,96
        /// Millionen auf 56 000.
        ///
        /// Der Einsteiger merkt nichts: Zwei Stätten kosten 3 × 140 / 1000 = 0 Taler.
        /// Siehe docs/haendlerbremse-konzept.md im Godot-Repo für die Kalibrierung.
        /// </summary>
        public const int KapazitaetsunterhaltProMille = 140;

        /// <summary>
        /// Anteil des Ansehens, der jährlich verfällt, in Prozent. Ruhm verblaßt: Wer nichts nachlegt,
        /// sinkt zurück.
        ///
        /// <b>Warum es das braucht.</b> <c>PermaAnsehen</c> war ein reines Konto - die
        /// Auslastungsbelohnung zahlte Jahr für Jahr bis zu <see cref="AnsehenMaxProJahr"/> Punkte
        /// dafür, dass die Werke ordentlich besetzt sind. Gemessen mit der Balancing-Harness stieg das
        /// Standesansehen eines schlichten Ritters ohne Amt dadurch linear um 5 Punkte im Jahr: 30 nach
        /// fünf Jahren, 80 nach fünfzehn, 205 nach vierzig, ohne Ende. Die Gerichtsschwellen
        /// (<c>GerichtsverhandlungManager</c>: 80 und 30) waren danach bedeutungslos.
        ///
        /// <b>Die Schwellen anzuheben wäre der falsche Griff gewesen.</b> Das Standesansehen der
        /// 390 KI-Spieler hat über 40 gemessene Jahre einen unveränderten Median von 29 - die Schwellen
        /// sind also auf das Feld geeicht und sitzen richtig. Nur der Mensch inflationierte, denn
        /// <c>PermaAnsehen</c> gibt es allein auf <c>HumSpieler</c>. Das ist dieselbe Asymmetrie, die
        /// beim Geldansehen beseitigt wurde (siehe <c>HumSpieler.AnsehenAktualisieren</c>), durch eine
        /// andere Tür.
        ///
        /// Der Verfall macht aus dem Konto einen Zustand mit Gleichgewicht: Bei einem Zufluss von
        /// <c>z</c> Punkten im Jahr pendelt sich das Ansehen bei rund <c>100 * z / Prozent</c> ein.
        /// Ganzzahlig gerechnet, also unterhalb von <c>100 / Prozent</c> ohne Wirkung - kleine Namen
        /// verblassen nicht. Er wirkt in beide Richtungen: Auch Schande verblaßt.
        ///
        /// Mit 10 % gemessen, jeweils im eingependelten Zustand: Ein Händler ohne Amt kommt auf ein
        /// Standesansehen von 60, ein Herzog mit Amt auf gut 300, der Median der KI liegt bei 29.
        /// Beide Gerichtsschwellen unterscheiden damit wieder - die 30 trennt den Betrieb vom Feld,
        /// die 80 den Adel vom Kaufmann.
        /// </summary>
        public const int AnsehensverfallProzent = 10;

        /// <summary>
        /// Taler Mehraufwand je Punkt permanenten Ansehens auf der Gewinnseite (aufwendige Hofhaltung).
        /// Der Kurs staffelt die Geltung nach Rang: Ein Herzog gewinnt mit aufwendiger Hofhaltung
        /// 5 Punkte im Jahr (50 000 Taler Mehraufwand), ein Fuerst 3, ein Graf 2, die Raenge darunter je
        /// einen. Ueber zehn Jahre sind das 50 bzw. 20 Punkte - die Groessenordnung eines KI-Ansehens
        /// (Amtsbonus mal 5, also 15 bis 60), erreichbar nur ueber Jahre.
        ///
        /// <b>Die Verlustseite hat einen eigenen, halbierten Kurs</b>
        /// (<see cref="AnsehenVerlustJeTalerHofhaltung"/>), damit sparsame Hofhaltung wirklich weh tut
        /// statt nur halb so stark zu wirken wie aufwendige.
        /// </summary>
        public const int AnsehenJeTalerHofhaltung = 10000;

        /// <summary>
        /// Taler Fehlbetrag je Punkt permanenten Ansehens auf der Verlustseite (sparsame Hofhaltung) -
        /// die Haelfte von <see cref="AnsehenJeTalerHofhaltung"/>, sonst waere Sparsamkeit nur ein
        /// Achtungszeichen und keine echte Bremse. Sparsam halbiert den standesgemaessen Aufwand, der
        /// Fehlbetrag ist also nie groesser als die Haelfte des Jahresaufwands - bei diesem Kurs reicht
        /// das fuer den Herzog exakt bis an den Deckel:
        ///
        /// <list type="bullet">
        /// <item>Baron: -7 500 Taler => -1 Punkt</item>
        /// <item>Graf: -12 500 Taler => -2 Punkte</item>
        /// <item>Fuerst: -17 500 Taler => -3 Punkte</item>
        /// <item>Herzog: -25 000 Taler => -5 Punkte (<see cref="AnsehenMaxProJahr"/> exakt erreicht)</item>
        /// </list>
        /// </summary>
        public const int AnsehenVerlustJeTalerHofhaltung = 5000;

        /// <summary>
        /// Obergrenze des Ansehensgewinns pro Jahr - <b>je Quelle</b>, nicht fuer beide zusammen:
        /// Hofhaltung und Auslastung werden getrennt gedeckelt und erst danach addiert. Im Jahr sind
        /// damit hoechstens <c>2 * AnsehenMaxProJahr</c> Punkte zu gewinnen (je der Deckel aus jeder
        /// Quelle).
        ///
        /// Nach unten ist die Grenze <b>nicht</b> symmetrisch, auch wenn der Clamp das nahelegt: Abziehen
        /// kann nur die Hofhaltung, mit einem eigenen, halbierten Kurs auf der Verlustseite
        /// (<see cref="AnsehenVerlustJeTalerHofhaltung"/>) - -1 bis -5 Punkte je nach Rang, beim Herzog
        /// exakt der Deckel. Weil beide Anteile <i>addiert</i> werden, kann eine gut ausgelastete Flotte
        /// den Sparmalus dennoch abfedern: Ein Herzog mit fuenf gut gefuehrten Produktionsplaetzen
        /// gleicht seinen eigenen Sparmalus (-5) gerade aus und behaelt 25 000 Taler, gewinnt dabei aber
        /// selbst keine Geltung mehr hinzu.
        ///
        /// Eine dritte Quelle - Bauwerksstiftungen (<c>BauwerkStiftenManager.FuehreStiftungAus</c>) -
        /// unterliegt demselben Deckel, wird aber ausserhalb dieser Methode verbucht und zaehlt daher
        /// nicht in die <c>2 * AnsehenMaxProJahr</c> oben ein.
        ///
        /// Ein gemeinsamer Topf haette die beiden Wege gegeneinander ausgespielt, obwohl Geltung sich
        /// erwirtschaften <i>oder</i> erkaufen laesst: Wer schon so viele Werke gut auslastet, wie der
        /// Deckel zulaesst, haette fuer aufwendige Hofhaltung 25 000 bis 50 000 Taler im Jahr gezahlt
        /// und nichts dafuer bekommen - und die Staffelung nach Betriebszahl waere genau dort stehen
        /// geblieben.
        ///
        /// Einstellige Werte, keine Zehner: Nach dem Wegfall des Geldansehens liegen KI-Werte bei 15
        /// bis 60, Aemterboni bei 3 bis 12. Bei 5 Punkten im Jahr braucht ein Spieler auf einem Weg
        /// allein drei bis zwoelf Jahre, um dorthin aufzuschliessen - Geltung waechst ueber Jahre und
        /// nicht in einem Zug.
        /// </summary>
        public const int AnsehenMaxProJahr = 5;

        /// <summary>Ab dieser Auslastung gilt ein einzelner Betrieb als gut gefuehrt.</summary>
        public const int AuslastungsschwelleProzent = 80;

        /// <summary>
        /// Berechnet alle Kostenpositionen des aktiven Spielers, zahlt Zölle an die
        /// Zollburg-Besitzer aus, zieht die Gesamtkosten von den Talern des Spielers ab
        /// und setzt seine Umsätze zurück.
        /// </summary>
        [PublicAPI]
        public AbrechnungsErgebnis ErstelleAbrechnungFuerAktivenSpieler()
        {
            var ergebnis = new AbrechnungsErgebnis();
            var spieler = SW.Dynamisch.GetAktHum();

            #region Arbeiter-, Betriebs- und Transportkosten

            int gutAusgelasteteBetriebe = 0;

            long staettenGesamt = 0;

            for (int stadtId = 1; stadtId < SW.Statisch.GetMaxStadtID(); stadtId++)
            {
                for (int slot = 0; slot < SW.Statisch.GetMaxProdSlots(); slot++)
                {
                    var produktionsslot = spieler.GetProduktionsslot(stadtId, slot);

                    if (produktionsslot.GetTaetigkeit() == (int)EnumProduktionsslotAktionsart.Produzieren)
                    {
                        int rohstoffId = produktionsslot.GetProduktionRohstoff();

                        ergebnis.Arbeiterkosten += produktionsslot.GetProduktionArbeiter() * SW.Dynamisch.GetRohstoffwithID(rohstoffId).GetWSArbeiterpreis();
                        ergebnis.Betriebskosten += produktionsslot.GetProduktionStaetten() * SW.Dynamisch.GetRohstoffwithID(rohstoffId).GetWSEinzelpreis();

                        // Fuer den Kapazitaetsunterhalt weiter unten - hier mitgezaehlt, statt in einer
                        // zweiten Schleife ueber dieselben Slots.
                        staettenGesamt += produktionsslot.GetProduktionStaetten();

                        // Auslastung = gesetzte zu benoetigten Arbeitern, dasselbe Verhaeltnis, mit dem
                        // Produktionsslot.GetProduktion den Ertrag anteilig senkt. Jeder Betrieb wird
                        // fuer sich bewertet, nicht als Mittel ueber alle produzierenden: Ein Mittel
                        // waere massstabsfrei gewesen - ein einziger gut besetzter Betrieb haette so
                        // viel gebracht wie zwanzig - und das Stilllegen eines schwachen Betriebs haette
                        // den eigenen Schnitt sogar gehoben, weil er aus der Messbasis fiel. Der Anreiz
                        // waere gewesen, die Messbasis zu verkleinern statt gut zu wirtschaften.
                        var wareSlot = SW.Dynamisch.GetRohstoffwithID(rohstoffId);
                        int proStaette = wareSlot.GetWerkstaetten() > 0
                            ? wareSlot.GetArbeiter() / wareSlot.GetWerkstaetten()
                            : 1;
                        int benoetigt = produktionsslot.GetProduktionStaetten() * proStaette;

                        // Ganzzahlig statt in Gleitkomma: Der Arbeiterdeckel liegt bei 99, ein Ueberlauf
                        // ist ausgeschlossen, und der Kern der Oekonomie bleibt exakt reproduzierbar.
                        //
                        // benoetigt > 0 ist dabei mehr als eine Nullpruefung fuer die Division: Wolle,
                        // Fell und Rum brauchen 1 Arbeiter je 2 Werkstaetten, ganzzahlig also 0 je
                        // Staette. Ohne diese Bedingung raeumte jeder Produktionsplatz dieser drei Waren
                        // die Schwelle ohne einen einzigen Arbeiter - Geltung zum Nulltarif, und zwar
                        // gerade fuer die Waren, die keine Auslastung kennen.
                        if (benoetigt > 0 &&
                            100 * produktionsslot.GetProduktionArbeiter() / benoetigt >= AuslastungsschwelleProzent)
                            gutAusgelasteteBetriebe++;
                    }

                    if (produktionsslot.GetTaetigkeit() == (int)EnumProduktionsslotAktionsart.Verkaufen ||
                        produktionsslot.GetTaetigkeit() == (int)EnumProduktionsslotAktionsart.PermanentVerkaufen)
                    {
                        int karawaneId = spieler.GetKarawaneInStadtX(stadtId);
                        int anzahlExportiert = produktionsslot.GetVerkaufAnzahl();

                        if (anzahlExportiert != 0)
                        {
                            // anzahlExportiert - 1, damit bei z. B. 400 Waren und Kapazität 100 noch 4 Fuhren und nicht 5 genommen werden
                            double bruch = (anzahlExportiert - 1) / SW.Statisch.GetKarawane(karawaneId).Kapazitaet;
                            double anzahlFuhren = Convert.ToInt32(bruch) + 1;

                            ergebnis.Transportkosten += Convert.ToInt32(SW.Statisch.GetKarawane(karawaneId).Fixpreis + SW.Statisch.GetKarawane(karawaneId).PreisProStueck * anzahlFuhren);
                        }
                    }
                }
            }

            ergebnis.Gesamtkosten += ergebnis.Arbeiterkosten + ergebnis.Betriebskosten + ergebnis.Transportkosten;

            #endregion

            #region Verkaufssteuern (angezeigt wird der volle Betrag, abgezogen der um die Steuerhinterziehung reduzierte)

            for (int stadtId = 1; stadtId < SW.Statisch.GetMaxStadtID(); stadtId++)
            {
                ergebnis.Verkaufssteuern += Convert.ToInt32(SW.Dynamisch.GetStadtwithID(stadtId).GetUmsatzsteuer() * spieler.GetUmsatzInStadtX(stadtId));
            }

            double steuerhinterziehung = 0;

            if (spieler.CheckPrivilegX(27))
                steuerhinterziehung = 0.2;

            if (spieler.CheckPrivilegX(28))
                steuerhinterziehung = 0.4;

            if (spieler.CheckPrivilegX(29))
                steuerhinterziehung = 0.6;

            int gezahlteVerkaufssteuern = Convert.ToInt32(ergebnis.Verkaufssteuern * (1 - steuerhinterziehung));
            ergebnis.Gesamtkosten += gezahlteVerkaufssteuern;

            #endregion

            #region Informanten und Saboteure

            for (int i = 1; i < SW.Statisch.GetMaxKIID(); i++)
            {
                ergebnis.Informantenkosten += spieler.GetAktiveSpionage(i).GetKosten();
                ergebnis.Saboteurekosten += spieler.GetAktiveSabotage(i).GetKosten();
            }

            ergebnis.Gesamtkosten += ergebnis.Informantenkosten + ergebnis.Saboteurekosten;

            #endregion

            #region Kreditzinsen

            for (int i = 0; i < SW.Statisch.GetMaxKredite(); i++)
            {
                if (spieler.GetKreditMitID(i).GetTaler() != 0)
                    ergebnis.Kreditzinsen += Convert.ToInt32(spieler.GetKreditMitID(i).GetTaler() * spieler.GetKreditMitID(i).GetZinsen() / 100);
            }

            ergebnis.Gesamtkosten += ergebnis.Kreditzinsen;

            #endregion

            #region Kirchenzehnt

            int gesamtUmsatz = 0;

            if (spieler.CheckPrivilegX(16) == false)
            {
                for (int stadtId = 1; stadtId < SW.Statisch.GetMaxStadtID(); stadtId++)
                    gesamtUmsatz += spieler.GetUmsatzInStadtX(stadtId);
            }

            ergebnis.Kirchenzehnt = Convert.ToInt32(SW.Statisch.GetKirchenzehnt() * gesamtUmsatz);
            ergebnis.Gesamtkosten += ergebnis.Kirchenzehnt;

            #endregion

            #region Zölle (Zollburg-Besitzer erhalten ihren Anteil sofort ausgezahlt)

            // Privilegien: Zollfreiheit (23) bzw. 50%-Chance auf Zollfreiheit (31). Ist der Spieler zollfrei,
            // zahlt er keinen Zoll – dann erhalten auch die Zollburg-Besitzer keinen Anteil ausgezahlt.
            bool zollfrei = spieler.CheckPrivilegX(23) || (spieler.CheckPrivilegX(31) && SW.Statisch.Rnd.Next(0, 2) == 0);

            if (!zollfrei)
            {
                for (int slot = 0; slot < SW.Statisch.GetMaxProdSlots(); slot++)
                {
                    for (int stadtId = 1; stadtId < SW.Statisch.GetMaxStadtID(); stadtId++)
                    {
                        var produktionsslot = spieler.GetProduktionsslot(stadtId, slot);

                        if (produktionsslot.GetTaetigkeit() != (int)EnumProduktionsslotAktionsart.Verkaufen &&
                            produktionsslot.GetTaetigkeit() != (int)EnumProduktionsslotAktionsart.PermanentVerkaufen)
                            continue;

                        int startland = SW.Dynamisch.GetStadtwithID(stadtId).GetLandID();
                        int zielland = SW.Dynamisch.GetStadtwithID(produktionsslot.GetVerkaufStadt()).GetLandID();

                        if (startland == zielland)
                            continue;

                        // Es wird mindestens eine Grenze überschritten
                        int grundumsatz = SW.Dynamisch.GetRohstoffwithID(produktionsslot.GetVerkaufRohstoff()).GetPreisStd() * produktionsslot.GetVerkaufAnzahl();

                        var zollburgStart = SW.Dynamisch.GetZollburgWithIDx(SW.Dynamisch.GetLandWithID(startland).GetZollburgIndex());
                        var zollburgZiel = SW.Dynamisch.GetZollburgWithIDx(SW.Dynamisch.GetLandWithID(zielland).GetZollburgIndex());

                        double zollsatzStart = zollburgStart.Zoll;
                        double zollsatzZiel = zollburgZiel.Zoll;

                        if (zollburgStart.Besitzer == SW.Dynamisch.GetAktiverSpieler())
                            zollsatzStart = 0;
                        else
                            ZahleZollAusUndSammle(zollburgStart.Besitzer, Convert.ToInt32(zollsatzStart * grundumsatz));

                        if (zollburgZiel.Besitzer == SW.Dynamisch.GetAktiverSpieler())
                            zollsatzZiel = 0;
                        else
                            ZahleZollAusUndSammle(zollburgZiel.Besitzer, Convert.ToInt32(zollsatzZiel * grundumsatz));

                        ergebnis.Zollkosten += Convert.ToInt32((zollsatzStart + zollsatzZiel) * grundumsatz);
                    }
                }
            }

            ergebnis.Gesamtkosten += ergebnis.Zollkosten;

            #endregion

            #region Sold

            foreach (Stuetzpunkt stuetzpunkt in SW.Dynamisch.GetStuetzpunkte())
            {
                if (stuetzpunkt.Besitzer != SW.Dynamisch.GetAktiverSpieler())
                    continue;

                foreach (Einheit einheit in stuetzpunkt.Einheiten)
                    ergebnis.Sold += einheit.Basispreis;
            }

            ergebnis.Gesamtkosten += ergebnis.Sold;

            #endregion

            #region Unterhalt

            // Progressiv statt linear: Der n-te Betrieb kostet das n-Fache des ersten, die Summe waechst
            // also quadratisch. Bewusst linear-progressiv und nicht geometrisch wie die Kaufpreisstaffel
            // (HandelsManager.SteigerungProzent, 125 % je Betrieb) - ueber zwanzig Stufen waere das
            // Faktor 86 und wuerde Expansion nicht bremsen, sondern verbieten.
            int werkstaetten = spieler.ZaehleWerkstaetten();

            ergebnis.Unterhalt = GrundunterhaltProWerkstatt * werkstaetten * (werkstaetten + 1) / 2;
            ergebnis.Gesamtkosten += ergebnis.Unterhalt;

            // Der Kapazitaetsunterhalt: dieselbe Dreieckszahl, aber ueber die betriebenen Staetten und
            // in Promille. long, weil das Produkt bei 84 Werkstaetten mal 99 Arbeitern die int-Grenze
            // sonst streifen koennte, bevor durch 1000 geteilt wird.
            ergebnis.Kapazitaetsunterhalt =
                (int)(staettenGesamt * (staettenGesamt + 1) / 2 * KapazitaetsunterhaltProMille / 1000);
            ergebnis.Gesamtkosten += ergebnis.Kapazitaetsunterhalt;

            // Der Faktor, sofern einer im Dienst steht. Er ist die einzige laufende Ausgabe, die der
            // Spieler mit einem Satz wieder los ist - und deshalb ein eigener Posten und kein
            // Unterhalt.
            var faktor = new FaktorManager();

            if (faktor.IstAngestellt())
            {
                ergebnis.Faktorlohn = faktor.GetJahreslohn();
                ergebnis.Gesamtkosten += ergebnis.Faktorlohn;
            }

            #endregion

            #region Hofhaltung

            // Die einzige laufende Ausgabe, die nicht an der Aktivität hängt, sondern am Stand. Sie ist
            // der Preis für das, was der Titel öffnet: die höheren Ämterebenen (GetMinTitelStadtEbene
            // und Geschwister) - und damit die Privilegien, mit denen sich Steuer, Zehnt und Zoll wieder
            // abschalten lassen. Titellose Spieler zahlen nichts, der Spielanfang bleibt unberührt.
            ergebnis.Hofhaltung = SW.Statisch.GetTitelX(spieler.GetTitel()).GetJahresaufwand()
                                  * HofhaltungFaktorProzent(spieler.GetHofhaltungAbweichung()) / 100;
            ergebnis.Gesamtkosten += ergebnis.Hofhaltung;

            // Der Mehr- oder Minderaufwand gegenueber dem standesgemaessen Satz sowie eine gut
            // ausgelastete Werkstattflotte schlagen sich in permanentem Ansehen nieder - der Waehrung, in
            // der auch Kirchenaustritt (-100), Kerker und Pranger rechnen. Jeder Anteil wird fuer sich
            // auf AnsehenMaxProJahr gedeckelt und erst danach addiert - damit Geltung ueber Jahre
            // waechst und nicht in einem Zug, ohne dass die beiden Wege einander verdraengen.
            int standesgemaess = SW.Statisch.GetTitelX(spieler.GetTitel()).GetJahresaufwand();
            int abweichungTaler = ergebnis.Hofhaltung - standesgemaess;
            int hofhaltungPunkte = 0;

            if (abweichungTaler != 0)
            {
                // Gewinn- und Verlustseite laufen ueber getrennte Kurse (siehe AnsehenVerlustJeTalerHofhaltung):
                // Sparsam soll wirklich weh tun und nicht nur halb so stark wirken wie aufwendig.
                int kurs = abweichungTaler > 0 ? AnsehenJeTalerHofhaltung : AnsehenVerlustJeTalerHofhaltung;
                hofhaltungPunkte = abweichungTaler / kurs;

                // Sparen kostet auch dann Geltung, wenn der Betrag fuer einen vollen Punkt nicht reicht.
                if (hofhaltungPunkte == 0)
                    hofhaltungPunkte = abweichungTaler > 0 ? 1 : -1;
            }

            if (hofhaltungPunkte > AnsehenMaxProJahr)
                hofhaltungPunkte = AnsehenMaxProJahr;
            else if (hofhaltungPunkte < -AnsehenMaxProJahr)
                hofhaltungPunkte = -AnsehenMaxProJahr;

            // Gut gefuehrte Werke bringen Geltung, und zwar je Betrieb: Wer zwanzig Werke gut auslastet,
            // hat mehr geleistet als wer eines auslastet. Bewusst mit hoher Schwelle - eine Auszeichnung,
            // kein Grundzustand. Nur nach oben wirksam: Ein stillgelegter oder schlecht besetzter Betrieb
            // bringt nichts, kann den Wert aber auch nie heben.
            int auslastungPunkte = gutAusgelasteteBetriebe > AnsehenMaxProJahr
                ? AnsehenMaxProJahr
                : gutAusgelasteteBetriebe;

            // Erst nach dem Deckel addieren, siehe AnsehenMaxProJahr: Erwirtschaften und Erkaufen sind
            // zwei Wege zur Geltung und nicht einer - aus einem gemeinsamen Topf verdraengten sie sich
            // gegenseitig, und die Hofhaltung waere fuer genau den Spieler wertlos geworden, auf den sie
            // zielt: den reichen, der seine Werke ohnehin gut fuehrt.
            int punkte = hofhaltungPunkte + auslastungPunkte;

            // Erst verblasst der alte Ruhm, dann zaehlen die Taten des Jahres. Diese Reihenfolge
            // haelt den Zufluss unangetastet - wer die vollen Punkte aus Hofhaltung und Auslastung
            // erwirbt, bekommt sie auch gutgeschrieben, und der Verfall trifft nur, was schon da
            // war. Wirkt in beide Richtungen: Bei negativem Ansehen bewegt der Abzug den Wert zur
            // Null hin, Schande verblasst also ebenfalls.
            int verfall = spieler.GetPermaAnsehen() * AnsehensverfallProzent / 100;

            if (verfall != 0)
                spieler.ErhoehePermaAnsehen(-verfall);

            if (punkte != 0)
                spieler.ErhoehePermaAnsehen(punkte);

            #endregion

            spieler.ErhoeheTaler(-ergebnis.Gesamtkosten);

            // Statistik (Issue #19): entrichtete Steuern (Verkaufssteuern + Kirchenzehnt) und Zölle mitzählen
            // sowie den Gesamtumsatz dieses Jahres aufsummieren, bevor die Umsätze zurückgesetzt werden.
            var statistik = spieler.GetSpielerStatistik();
            statistik.HaentrichteteSteuern += gezahlteVerkaufssteuern + ergebnis.Kirchenzehnt;
            statistik.HaentrichteteZoelle += ergebnis.Zollkosten;

            // Umsätze des Spielers aufsummieren und wieder auf 0 setzen
            for (int stadtId = 1; stadtId < SW.Statisch.GetMaxStadtID(); stadtId++)
            {
                statistik.SoGesamtumsatz += spieler.GetUmsatzInStadtX(stadtId);
                spieler.SetUmsatzInStadtX(0, stadtId);
            }

            return ergebnis;
        }

        /// <summary>
        /// Der Aufwand als Prozentsatz des standesgemaessen: sparsam die Haelfte, standesgemaess genau,
        /// aufwendig das Doppelte.
        /// </summary>
        public static int HofhaltungFaktorProzent(int abweichung)
        {
            if (abweichung < 0)
                return 50;

            return abweichung > 0 ? 200 : 100;
        }

        /// <summary>
        /// Zahlt einem Zollburg-Besitzer seinen Zollanteil aus und summiert ihn – sofern der Besitzer ein
        /// menschlicher Spieler ist – in dessen Zolleinnahmen-Zähler, damit er ihm zu Zugbeginn gemeldet werden kann.
        /// </summary>
        private static void ZahleZollAusUndSammle(int besitzerId, int betrag)
        {
            SW.Dynamisch.GetSpWithID(besitzerId).ErhoeheTaler(betrag);

            if (besitzerId < SW.Statisch.GetMinKIID())
                SW.Dynamisch.GetHumWithID(besitzerId).ZolleinnahmenGesammelt += betrag;
        }
    }
}
