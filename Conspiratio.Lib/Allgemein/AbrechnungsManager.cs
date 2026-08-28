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
        /// Unterhalt des ersten Betriebs; der n-te kostet das n-Fache. Zwischen zwei Schranken gemessen:
        ///
        /// Nach unten: Zwei Betriebe kosten drei Grundeinheiten, also 300 Taler - Rauschen gemessen am
        /// Startkapital, und mit Abstand unter den 500 Talern, die der Einsteigertest zulaesst.
        ///
        /// Nach oben: der Deckungsbeitrag einer ausgelasteten Werkstatt. Aus
        /// <see cref="Rohstoffe.Rohstoff.GetWSProdProWS"/> (90 % davon bei richtiger Arbeiterzahl) gegen
        /// <see cref="Rohstoffe.Rohstoff.GetWSEinzelpreis"/> und
        /// <see cref="Rohstoffe.Rohstoff.GetWSArbeiterpreis"/> traegt die schwaechste Ware in einem bis
        /// zum Arbeiterdeckel besetzten Produktionsplatz 8 474 Taler im Jahr, die staerkste 20 438. Bei
        /// 100 Talern Grundunterhalt kostet selbst der 28. Betrieb - so viele Produktionsplaetze, wie
        /// 14 Staedte mal <c>GetMaxProdSlots</c> ueberhaupt hergeben - 2 800 Taler und damit ein Drittel
        /// des schwaechsten und ein Siebtel des staerksten Deckungsbeitrags. Expansion bleibt damit
        /// lohnend, wird aber spuerbar teurer.
        /// </summary>
        public const int GrundunterhaltProWerkstatt = 100;

        /// <summary>
        /// Taler Mehraufwand je Punkt permanenten Ansehens. Der Kurs staffelt die Geltung nach Rang:
        /// Ein Herzog gewinnt mit aufwendiger Hofhaltung 5 Punkte im Jahr (50 000 Taler Mehraufwand),
        /// ein Fuerst 3, ein Graf 2, die Raenge darunter je einen. Ueber zehn Jahre sind das 50 bzw. 20
        /// Punkte - die Groessenordnung eines KI-Ansehens (Amtsbonus mal 5, also 15 bis 60), erreichbar
        /// nur ueber Jahre.
        /// </summary>
        public const int AnsehenJeTalerHofhaltung = 10000;

        /// <summary>
        /// Obergrenze des Ansehensgewinns pro Jahr. Einstellige Werte, keine Zehner: Nach dem Wegfall
        /// des Geldansehens liegen KI-Werte bei 15 bis 60, Aemterboni bei 3 bis 12. Bei 5 Punkten im
        /// Jahr braucht ein Spieler drei bis zwoelf Jahre, um dorthin aufzuschliessen - Geltung waechst
        /// ueber Jahre und nicht in einem Zug.
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
            // der auch Kirchenaustritt (-100), Kerker und Pranger rechnen. Beide Anteile teilen sich
            // dieselbe Jahresobergrenze und werden deshalb erst summiert und dann gemeinsam gedeckelt,
            // damit Geltung ueber Jahre waechst und nicht in einem Zug.
            int standesgemaess = SW.Statisch.GetTitelX(spieler.GetTitel()).GetJahresaufwand();
            int abweichungTaler = ergebnis.Hofhaltung - standesgemaess;
            int punkte = 0;

            if (abweichungTaler != 0)
            {
                punkte = abweichungTaler / AnsehenJeTalerHofhaltung;

                // Sparen kostet auch dann Geltung, wenn der Betrag fuer einen vollen Punkt nicht reicht.
                if (punkte == 0)
                    punkte = abweichungTaler > 0 ? 1 : -1;
            }

            // Gut gefuehrte Werke bringen Geltung, und zwar je Betrieb: Wer zwanzig Werke gut auslastet,
            // hat mehr geleistet als wer eines auslastet. Bewusst mit hoher Schwelle - eine Auszeichnung,
            // kein Grundzustand. Nur nach oben wirksam: Ein stillgelegter oder schlecht besetzter Betrieb
            // bringt nichts, kann den Wert aber auch nie heben. Den gemeinsamen Jahresdeckel zieht der
            // Block darunter.
            punkte += gutAusgelasteteBetriebe;

            if (punkte > AnsehenMaxProJahr)
                punkte = AnsehenMaxProJahr;
            else if (punkte < -AnsehenMaxProJahr)
                punkte = -AnsehenMaxProJahr;

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
