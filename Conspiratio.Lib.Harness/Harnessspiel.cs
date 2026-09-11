using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Conspiratio.Lib.Allgemein;
using Conspiratio.Lib.Gameplay.Niederlassung;
using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Harness
{
    internal sealed class JaStub : IYesNoQuestion
    {
        public Task<DialogResultGame> ShowDialogText(string frage, string ja = "Ja", string nein = "Nein")
            => Task.FromResult(DialogResultGame.Yes);
    }

    internal sealed class TextStub : IShowText
    {
        public Task ShowDialog(string text) => Task.CompletedTask;
    }

    /// <summary>Was ein Jahr eingebracht und gekostet hat.</summary>
    public sealed class Jahreszeile
    {
        public int Jahr;
        public int Taler;
        public int Vermoegen;
        public int Produziert;
        public int Verkauft;
        public int Erloes;
        public int Unterhalt;

        /// <summary>Die Loehne dieses Jahres - die Groesse, an der das Arbeiterverhaeltnis haengt.</summary>
        public int Arbeiterkosten;

        /// <summary>Die gefahrene Auslastung in Prozent - ohne <c>--adaptiv</c> immer 100.</summary>
        public int Auslastung = 100;
        public int Hofhaltung;
        public int Gesamtkosten;
        public int Strafen;

        /// <summary>Mittlerer Marktabschlag der belieferten Staedte in Prozentpunkten.</summary>
        public int Abschlag;

        /// <summary>Erreichter Rang - ohne ihn ist die Hofhaltung nicht zu deuten.</summary>
        public string Titel;

        /// <summary>Der Kapazitaetsunterhalt dieses Jahres (AbrechnungsErgebnis.Kapazitaetsunterhalt).</summary>
        public int Kapazitaet;

        /// <summary>Amtseinkommen dieses Jahres.</summary>
        public int Amtseinkommen;

        /// <summary>Name des gefuehrten Amtes.</summary>
        public string Amt;

        /// <summary>Das rohe Ansehen - hier steckt der Ansehensbonus des Amtes bereits drin.</summary>
        public int Ansehen;

        /// <summary>
        /// Ansehen zuzueglich des Titelbonus (<c>Spieler.GetStandesAnsehen</c>). Gilt ausdruecklich nur
        /// dort, wo ueber eine Person geurteilt wird: vor Gericht und im Schuldenprozess. In Wahlen
        /// zaehlt er bewusst nicht.
        /// </summary>
        public int StandesAnsehen;
    }

    /// <summary>Wie der Betrieb aufgebaut wird, bevor die Jahre laufen.</summary>
    public sealed class Aufbau
    {
        public int Staedte = 1;
        public int SlotsProStadt = 1;
        public int StartTaler = 50_000;
        public int Lagerausbauten = 8;

        /// <summary>
        /// Staetten je Linie. 0 bedeutet: so viele, wie Arbeiter und Lager hergeben - die naive
        /// Strategie. Jeder andere Wert drosselt und macht damit die eigentlich interessante Frage
        /// messbar: Wo liegt der Betriebspunkt, an dem der Markt die Menge noch aufnimmt?
        /// </summary>
        public int StaettenJeLinie;

        /// <summary>
        /// Alle Linien auf diese Ware setzen statt auf die Hauptproduktion der jeweiligen Stadt.
        /// 0 heisst: wie bisher die Stadtware. Noetig, um eine bestimmte Ware zu messen - Wolle,
        /// Fell und Rum sind die einzigen drei mit einem Arbeiterverhaeltnis unter eins, und ob
        /// eine davon vorkommt, entscheidet sonst die Stadtliste.
        /// </summary>
        public int Ware;

        /// <summary>
        /// Wie die Linien besetzt werden: "voll" (Staetten mal max(1, Verhaeltnis) - die bisherige
        /// und weiterhin voreingestellte Rechnung), "exakt" (Staetten mal Arbeiter durch
        /// Werkstaetten, also genau was die Ware verlangt) oder "null" (gar keine Arbeiter).
        ///
        /// "null" ist kein Unsinn, sondern der Messfall: Vor der Behebung des Arbeiterbedarfs
        /// lieferten Wolle, Fell und Rum damit die volle Menge zum Nulltarif.
        /// </summary>
        public string Besetzung = "voll";

        /// <summary>
        /// Produktionsfertigkeit, auf die jede betriebene Ware gesetzt wird (0 bis 100). Sie hebt in
        /// <c>Produktionsslot.GetProduktion</c> den unteren Rand der Jahresschwankung, im Mittel also
        /// den Ertrag um bis zu rund 5 %. Ob das ueber eine Partie etwas ausmacht, war die Frage,
        /// derentwegen es diesen Schalter gibt - das Konzept verlangt sie ausdruecklich, und der
        /// Harness kannte die Groesse bis dahin gar nicht.
        /// </summary>
        public int Fertigkeit;

        /// <summary>
        /// Den Weg zur Fertigkeit bezahlen, statt sie zu schenken: je Ware so viele Lehrstunden, wie
        /// noetig sind, ueber <c>ProduktionsfertigkeitManager.NimmLehrstunde</c> und damit mit der
        /// echten Preisstaffel. Erst damit laesst sich sagen, ob sich das Training rechnet.
        /// </summary>
        public bool Lehrgeld;

        /// <summary>
        /// Die Kapazitaet nach dem Jahresergebnis nachfuehren, statt immer auf Anschlag zu
        /// produzieren. Ohne das haelt der Haendler die volle Menge auch dann, wenn eine Abgabe
        /// ihn ruiniert - und jede Bremsmessung ueberschaetzt den Schaden, weil ein Mensch
        /// laengst gedrosselt haette.
        /// </summary>
        public bool Adaptiv;
        public bool Vollstaendig;

        /// <summary>
        /// Verkaufen, wo die Ware teuer ist, statt dort, wo sie entsteht. Eine Stadt zahlt fuer ihre
        /// eigene Hauptproduktion am wenigsten - vor Ort zu verkaufen ist deshalb der schlechteste
        /// Preis des Spiels und war der Grund, aus dem die erste Fassung dieser Harness in jeder
        /// Konfiguration ins Minus lief, auch bei 1 % Marktabschlag.
        ///
        /// Im Exportbetrieb produziert Slot 0 und Slot 1 traegt den Verkaufsauftrag; je Stadt bleibt
        /// damit eine Produktionslinie statt zweier.
        /// </summary>
        public bool Export;

        /// <summary>
        /// Den Adelsweg mitgehen: Wohnsitze kaufen, sobald sie bezahlbar sind. Ohne das steigt der
        /// Spieler nie im Rang - und die Hofhaltung, eine der Spaetspiel-Geldsenken, bleibt fuer immer
        /// bei 0. Die Titelleiter haengt naemlich nicht am Vermoegen allein: Landherr verlangt ein
        /// Landhaus, Baron einen Landsitz, Freiherr eine Villa, Graf und hoeher zusaetzlich
        /// Stuetzpunkte. Ein reiner Haendler zahlt diese Senke also nie.
        /// </summary>
        public bool Adel;

        /// <summary>
        /// Nur so viel produzieren, wie eine Stadt im Jahr verbraucht, statt so viel wie Arbeiter und
        /// Lager hergeben. Die naive Strategie schuettet die Zielmaerkte zu und verkauft danach zum
        /// halben Preis - was gemessen wird, ist dann die Strafe fuer Ueberproduktion und nicht das
        /// Balancing.
        /// </summary>
        public bool Marktklug;

        /// <summary>
        /// Aemter uebernehmen, soweit der Rang sie erlaubt. Das ist die Ertragsseite des Adelswegs, die
        /// bis jetzt fehlte: Titel sind Zugangsvoraussetzung fuer Aemter
        /// (<c>GetMinTitelStadtEbene</c> 1, <c>GetMinTitelLandEbene</c> 3,
        /// <c>GetMinTitelReichsEbene</c> 5), und Aemter zahlen ein Jahreseinkommen und geben Ansehen.
        ///
        /// Uebernommen wird ueber <c>CheatManager.UebernehmeAmt</c> - denselben Weg, den der E2E-Treiber
        /// fuer <c>--zustaende</c> nutzt, weil er Amt und Gebiet zusammen setzt und der verdraengten KI
        /// ihr altes Amt zurueckgibt. <b>Das ist eine Abkuerzung:</b> Im Spiel wird ein Amt in einer Wahl
        /// gewonnen und kann durch Amtsenthebung wieder verloren gehen. Gemessen wird hier also die
        /// <i>Groesse</i> des Amtsertrags, nicht die Wahrscheinlichkeit, eines zu halten.
        /// </summary>
        public bool Aemter;

        /// <summary>
        /// Der echte politische Weg zum Amt statt der Abkuerzung: Der Spieler bewirbt sich auf freie
        /// Aemter, die Wahlen werden ausgezaehlt, Amtsenthebungen laufen, und KI-Spieler sterben und
        /// machen dabei Stellen frei. Das beantwortet die Frage, die <see cref="Aemter"/> offen laesst -
        /// nicht wie viel ein Amt traegt, sondern wie lange man eines haelt.
        ///
        /// Zieht den KI-Jahreswechsel zwingend mit sich: Ohne ihn altert keine KI, es stirbt keine, und
        /// kein Amt wird je frei.
        /// </summary>
        public bool Wahlen;
    }

    /// <summary>
    /// Eine Partie, die nicht gespielt, sondern gerechnet wird: Der Betrieb wird direkt aufgebaut, danach
    /// laufen die Jahre in der Reihenfolge des Clients ab.
    ///
    /// <b>Wozu:</b> Ein E2E-Lauf kann das Spaetspiel nicht beurteilen. Sein Treiber bearbeitet einen
    /// Produktionsslot in einer Stadt, und der Arbeiterdeckel von <c>GetMaxArbeiterAnzahl()</c> begrenzt
    /// ihn dort. Das Spiel erlaubt dagegen <c>GetMaxProdSlots()</c> = 2 Auftraege je Stadt ueber
    /// 14 Staedte, also bis zu 28 Produktionslinien - der Treiber sieht rund ein Achtundzwanzigstel der
    /// moeglichen Wirtschaft. Mechaniken, die erst bei Millionen greifen (Gesetz #3, der Werkstatt-
    /// Aufschlag jenseits von <c>MaxSteigerungsstufen</c>), liegen damit ausserhalb seiner Reichweite.
    ///
    /// <b>Was hier bewusst fehlt:</b> Der Spieler stirbt nicht (die Sterbepruefung bleibt aus), es gibt
    /// keine Familien-, Gerichts- und Zufallsereignisse und keine Katastrophen. Das ist kein Versehen,
    /// sondern der Zweck: Diese Systeme streuen laut den E2E-Messungen um Zehntausende Taler und
    /// ueberdecken genau das, was hier gemessen werden soll. <see cref="Aufbau.Vollstaendig"/> schaltet
    /// den KI-Jahreswechsel zu, wenn man den Gegencheck will.
    /// </summary>
    public sealed class Harnessspiel
    {
        private const int SpielerId = 1;

        private readonly Aufbau _aufbau;
        private readonly HandelsManager _handel = new HandelsManager();
        private readonly RundenManager _runden = new RundenManager();

        /// <summary>Je Stadt die Ware des jeweiligen Slots - einmal beim Aufbau bestimmt.</summary>
        private readonly List<(int StadtId, int Slot, int RohstoffId)> _linien = new List<(int, int, int)>();

        /// <summary>Was der Aufbau an Lehrstunden gekostet hat - 0 ohne <c>--lehrgeld</c>.</summary>
        public int LehrgeldGezahlt { get; private set; }

        /// <summary>Wie weit der Betrieb gefahren wird, in Prozent der moeglichen Kapazitaet.</summary>
        public int Auslastung { get; private set; } = 100;

        /// <summary>Unter diese Auslastung geht der Haendler nicht - darunter lohnt kein Betrieb.</summary>
        private const int MindestAuslastung = 10;

        /// <summary>Ein Verlustjahr nimmt so viele Punkte Auslastung weg ...</summary>
        private const int SchrittRunter = 15;

        /// <summary>... ein Gewinnjahr gibt so viele zurueck. Asymmetrisch, damit es sich einpendelt.</summary>
        private const int SchrittHoch = 5;

        public Harnessspiel(Aufbau aufbau)
        {
            _aufbau = aufbau;
        }

        /// <summary>Der Grund, aus dem eine Linie nicht mehr Staetten betreibt - fuer den Bericht.</summary>
        public string BindendeGrenze { get; private set; } = "?";

        public int Staetten { get; private set; }

        public int Arbeiter { get; private set; }

        #region Aufbau

        public void Starte(int seed)
        {
            SW.Statisch.Initialisieren();

            // Der Seed wird ZWEIMAL gesetzt, wie im E2E-Treiber: einmal vor dem Aufbau, damit die
            // Welt selbst reproduzierbar entsteht, und einmal, wenn das Spiel steht. Ohne den ersten
            // Aufruf zogen CreateNewGame und die Spielererstellung aus einem ungeseedeten Strom -
            // der Haendlerweg war davon unbeeindruckt, der Adelsweg nicht: Dieselbe Kommandozeile
            // lieferte ueber mehrere Aufrufe 266 441, 367 734 und 187 274 Taler Median.
            SW.Statisch.SetRnd(seed);

            string pfad = Path.Combine(Path.GetTempPath(), "conspiratio-harness");
            Directory.CreateDirectory(pfad);

            new NewGameManager(pfad).CreateNewGame("Harness", 1, false, true, false, out _);
            SW.Dynamisch.Spielstand.Einstellungen.KiAggressivitaetProzent = 50;
            SW.UI.Initialisieren(new JaStub(), new TextStub(), null, null, null, null, null, null, null, null, null);

            var setup = new PlayerSetupManager();
            setup.Starte();
            setup.ErstelleSpieler("Harness", true, 3, SW.Statisch.GetRelKathID(), 5, true, 1);
            setup.Beende();
            SW.Dynamisch.SetAktiverSpieler(SpielerId);

            // Zum zweiten Mal, damit die gemessenen Jahre an derselben Stelle des Stroms beginnen,
            // egal wie viel der Aufbau verbraucht hat.
            SW.Statisch.SetRnd(seed);

            BaueBetrieb();
        }

        /// <summary>
        /// Schaltet je Stadt die ersten Werkstattplaetze frei, kauft Lagerraum dazu und stellt die
        /// Produktionsauftraege ein. Die Werkstatt wird direkt freigeschaltet statt gekauft - der Zweck
        /// ist, den Spieler an einen Zustand zu <i>setzen</i>, nicht ihn dorthin spielen zu lassen.
        /// </summary>
        private void BaueBetrieb()
        {
            var spieler = SW.Dynamisch.GetAktHum();
            spieler.SetTaler(_aufbau.StartTaler);

            int staedte = Math.Min(_aufbau.Staedte, SW.Statisch.GetMaxStadtID() - 1);
            // Im Exportbetrieb ist Slot 1 fuer den Verkaufsauftrag reserviert.
            int slots = _aufbau.Export ? 1 : Math.Min(_aufbau.SlotsProStadt, SW.Statisch.GetMaxProdSlots());

            for (int stadtId = 1; stadtId <= staedte; stadtId++)
            {
                for (int slot = 0; slot < slots; slot++)
                {
                    int platz = slot + 1;   // Werkstattplaetze zaehlen 1-basiert
                    int rohstoffId = _aufbau.Ware > 0
                        ? _aufbau.Ware
                        : SW.Dynamisch.GetStadtwithID(stadtId).GetSingleRohstoff(platz);

                    // Der Werkstattkauf wird vollstaendig gespiegelt, nicht nur freigeschaltet:
                    // HandelsManager.KaufeWerkstatt setzt neben Enabled auch die Rohstoff-ID und vor
                    // allem den Startlagerraum. Ohne den bleibt der Lagerplatz 0 - und weil der
                    // Lagerausbau multiplikativ ist (LagerraumManager: AktuellerLagerraum * Faktor),
                    // kommt er von 0 nie hoch. BuchManager wirft dann die gesamte Produktion weg,
                    // ohne dass es irgendwo auffiele. Genau das hat die erste Fassung getan.
                    var werkstatt = spieler.GetSpielerHatInStadtXWerkstaettenY(platz, stadtId);
                    werkstatt.SetEnabled(true);
                    werkstatt.SetRohstoffID(rohstoffId);
                    werkstatt.SetSkillX(1, SW.Statisch.GetStartLagerraum());
                    spieler.SetRohstoffrechteXZuY(rohstoffId, true);

                    KaufeLagerraum(stadtId, platz);

                    _handel.SetzeTaetigkeit(stadtId, slot, EnumProduktionsslotAktionsart.Produzieren);
                    _handel.SetzeProduktionsRohstoff(stadtId, slot, rohstoffId);

                    _linien.Add((stadtId, slot, rohstoffId));
                }
            }

            SetzeProduktionsauftraege();
            SetzeFertigkeit();
        }

        /// <summary>
        /// Bringt jede betriebene Ware auf <see cref="Aufbau.Fertigkeit"/> - geschenkt oder mit
        /// Lehrgeld bezahlt. Je Ware nur einmal, auch wenn mehrere Staedte sie herstellen: Das
        /// Koennen haengt am Spieler, nicht am Standort.
        /// </summary>
        private void SetzeFertigkeit()
        {
            if (_aufbau.Fertigkeit <= 0)
                return;

            var spieler = SW.Dynamisch.GetAktHum();
            var fertigkeit = new ProduktionsfertigkeitManager();
            var erledigt = new List<int>();
            int talerVorher = spieler.GetTaler();

            foreach (var (_, _, rohstoffId) in _linien)
            {
                if (erledigt.Contains(rohstoffId))
                    continue;

                erledigt.Add(rohstoffId);

                if (!_aufbau.Lehrgeld)
                {
                    spieler.SetProduktionsfertigkeit(rohstoffId, _aufbau.Fertigkeit);
                    continue;
                }

                // Stunde um Stunde, damit die Preisstaffel wirklich greift - 1 000 plus 500 je
                // bereits genommener Stunde derselben Ware. Reicht das Geld nicht, endet der
                // Unterricht hier; das ist die ehrliche Antwort und keine Ausnahme.
                while (spieler.GetProduktionsfertigkeit(rohstoffId) < _aufbau.Fertigkeit &&
                       fertigkeit.NimmLehrstunde(rohstoffId, out _))
                {
                }
            }

            LehrgeldGezahlt = talerVorher - spieler.GetTaler();
        }

        /// <summary>
        /// Kauft der Reihe nach das groesste bezahlbare Lagerangebot. Ohne Lager laeuft die Produktion
        /// ins Leere: <c>BuchManager</c> bezahlt sie und wirft weg, was nicht eingelagert werden kann.
        /// </summary>
        private void KaufeLagerraum(int stadtId, int platz)
        {
            for (int runde = 0; runde < _aufbau.Lagerausbauten; runde++)
            {
                var lager = new LagerraumManager(stadtId, platz);
                int bestes = -1;

                for (int angebot = 0; angebot < 3; angebot++)
                {
                    try
                    {
                        if (lager.GetPreis(angebot) <= SW.Dynamisch.GetAktHum().GetTaler())
                            bestes = angebot;
                    }
                    catch (IndexOutOfRangeException)
                    {
                        break;
                    }
                }

                if (bestes < 0 || !lager.Kaufe(bestes))
                    return;
            }
        }

        /// <summary>
        /// Wie viele Arbeiter eine Linie bekommt - siehe <see cref="Aufbau.Besetzung"/>.
        ///
        /// "exakt" rechnet wie die Lib seit der Behebung: erst nehmen, dann teilen. Ein
        /// Zwischenwert "Arbeiter je Staette" waere bei Wolle, Fell und Rum null und wuerde die
        /// Frage, um die es hier geht, gerade wegrunden.
        /// </summary>
        private int ErmittleArbeiter(Gameplay.Rohstoffe.Rohstoff ware, int staetten)
        {
            if (_aufbau.Besetzung == "null")
                return 0;

            if (_aufbau.Besetzung == "exakt")
                return staetten * ware.GetArbeiter() / ware.GetWerkstaetten();

            return staetten * Math.Max(1, ware.GetArbeiter() / ware.GetWerkstaetten());
        }

        /// <summary>
        /// Staettenzahl und Arbeiter je Linie - nach derselben Regel wie der E2E-Treiber: Es bindet,
        /// was zuerst greift, der Arbeiterdeckel oder der Lagerplatz. Arbeiter werden immer exakt im
        /// Verhaeltnis der Ware gesetzt; zu wenige senken den Ertrag anteilig, zu viele kosten nur Lohn.
        /// </summary>
        private void SetzeProduktionsauftraege()
        {
            var spieler = SW.Dynamisch.GetAktHum();
            int staettenGesamt = 0;
            int arbeiterGesamt = 0;
            int lagerBindet = 0;

            foreach (var (stadtId, slot, rohstoffId) in _linien)
            {
                var ware = SW.Dynamisch.GetRohstoffwithID(rohstoffId);

                int arbeiterProStaette = Math.Max(1, ware.GetArbeiter() / ware.GetWerkstaetten());
                int mengeProStaette = Math.Max(1, ware.GetWSProdProWS() * 9 / 10);

                int ausArbeitern = SW.Statisch.GetMaxArbeiterAnzahl() / arbeiterProStaette;
                int lagerplatz = spieler.ErmittleLagerplatzInStadt(stadtId, rohstoffId) * ware.GetLagermengeProQMeter();
                int ausLager = Math.Max(1, lagerplatz / mengeProStaette);

                int staetten = Math.Max(1, Math.Min(ausArbeitern, ausLager));

                if (_aufbau.StaettenJeLinie > 0)
                    staetten = Math.Min(staetten, _aufbau.StaettenJeLinie);

                // Marktklug: nicht mehr erzeugen, als eine Stadt im Jahr verbraucht. Alles darueber
                // bleibt im Zielmarkt liegen und drueckt den Preis, den man selbst erloest.
                if (_aufbau.Marktklug)
                    staetten = Math.Max(1, Math.Min(staetten, MittlererJahresbedarf() / mengeProStaette));

                // Die nachgefuehrte Auslastung sitzt zuletzt: Sie drosselt, was die Grenzen oben
                // erlauben, und nicht umgekehrt.
                if (_aufbau.Adaptiv)
                    staetten = Math.Max(1, staetten * Auslastung / 100);

                if (ausLager < ausArbeitern)
                    lagerBindet++;

                _handel.SetzeProduktionsStaetten(stadtId, slot, staetten);
                _handel.SetzeProduktionsArbeiter(stadtId, slot, ErmittleArbeiter(ware, staetten));

                var eingestellt = _handel.GetProduktionsslot(stadtId, slot);
                staettenGesamt += eingestellt.GetProduktionStaetten();
                arbeiterGesamt += eingestellt.GetProduktionArbeiter();
            }

            Staetten = staettenGesamt;
            Arbeiter = arbeiterGesamt;
            BindendeGrenze = lagerBindet == 0 ? "Arbeiter"
                : lagerBindet == _linien.Count ? "Lager"
                : "gemischt";
        }

        /// <summary>
        /// Fuehrt die Auslastung nach dem Jahresergebnis nach: Verlust drosselt, Gewinn baut wieder
        /// auf. Das ist eine Faustregel und keine optimale Strategie - sie soll nur verhindern, dass
        /// der Haendler auf Anschlag weiterproduziert, waehrend er ruiniert wird.
        ///
        /// <b>Ein Versatz steckt darin:</b> Im Exportbetrieb steht im Erloes das <i>Vorjahr</i> (der
        /// Karawanenzug rechnet erst dann ab), in den Kosten aber das laufende Jahr. Fuer eine
        /// Faustregel reicht das; wer den Betriebspunkt genau bestimmen will, darf sich darauf
        /// nicht verlassen.
        ///
        /// Das erste Jahr bleibt aussen vor: Da ist das Buch noch leer, der Erloes also 0, und der
        /// Haendler wuerde drosseln, bevor er ueberhaupt etwas verkauft hat.
        /// </summary>
        private void PasseAuslastungAn(Jahreszeile zeile)
        {
            if (!_aufbau.Adaptiv || zeile.Jahr <= SW.Statisch.StartJahr)
                return;

            if (zeile.Erloes < zeile.Gesamtkosten)
                Auslastung = Math.Max(MindestAuslastung, Auslastung - SchrittRunter);
            else
                Auslastung = Math.Min(100, Auslastung + SchrittHoch);
        }

        #endregion

        #region Ein Jahr

        /// <summary>
        /// Ein Jahr in der Reihenfolge, in der der Godot-Client es abwickelt (Kontor.cs): Das Buch zu
        /// Zugbeginn verbucht die Auftraege des <i>Vorjahres</i>, danach handelt der Spieler, dann folgen
        /// Kredite, Jahresabrechnung, Zugabschluss, die wirtschaftlich wirksamen Zugnachrichten und
        /// zuletzt das wirtschaftliche Rundenende.
        ///
        /// Diese Reihenfolge ist die wichtigste Eigenschaft der Harness. Weicht sie ab, beschreiben ihre
        /// Zahlen ein Spiel, das es nicht gibt.
        /// </summary>
        public Jahreszeile SpieleJahr()
        {
            var spieler = SW.Dynamisch.GetAktHum();
            var zeile = new Jahreszeile { Jahr = SW.Dynamisch.GetAktuellesJahr() };

            // 0. Zugbeginn im Sinne der Lib: aktualisiert Privilegien und Ansehen und setzt den
            //    Jahresdeckel fuer Stiftungs-Ansehen zurueck. Der Aufruf fehlte in der ersten Fassung -
            //    ohne ihn wird das Ansehen nie fortgeschrieben und der Deckel bleibt nach dem ersten
            //    Jahr fuer immer ausgeschoepft.
            _runden.BeginneZug();

            // 1. Das Buch verbucht Produktion und Exporterloese des Vorjahres.
            var buch = new BuchManager().ErstelleJahresbuchFuerAktivenSpieler();

            for (int rohstoffId = 1; rohstoffId < SW.Statisch.GetMaxRohID(); rohstoffId++)
                zeile.Produziert += buch.ProduzierteWaren[rohstoffId];

            // Im Exportbetrieb steht der Erloes des Vorjahres im Buch - der Verkauf selbst fand
            // beim Karawanenzug statt, nicht hier.
            if (_aufbau.Export)
            {
                for (int rohstoffId = 1; rohstoffId < SW.Statisch.GetMaxRohID(); rohstoffId++)
                {
                    zeile.Erloes += buch.ExportErloese[rohstoffId];
                    zeile.Verkauft += buch.ExportierteWaren[rohstoffId];
                }
            }

            // 2. Der Spieler handelt: Ware absetzen, dann neu produzieren lassen.
            if (_aufbau.Export)
                SetzeExportauftraege();
            else
                VerkaufeBestaende(zeile);

            SetzeProduktionsauftraege();

            // 3. Zugende
            new SchreibstubeManager().TilgeUeberfaelligeKredite();

            var abrechnung = new AbrechnungsManager().ErstelleAbrechnungFuerAktivenSpieler();
            zeile.Unterhalt = abrechnung.Unterhalt;
            zeile.Arbeiterkosten = abrechnung.Arbeiterkosten;
            zeile.Hofhaltung = abrechnung.Hofhaltung;
            zeile.Gesamtkosten = abrechnung.Gesamtkosten;
            zeile.Kapazitaet = abrechnung.Kapazitaetsunterhalt;

            _runden.SchliesseZugAb();

            // 4. Zugnachrichten, soweit sie auf das Vermoegen wirken.
            var nachrichten = new ZugNachrichtenManager();

            // Ohne die Titelverleihung bleibt der Spieler auf dem Anfangsrang - und damit stuende
            // die Hofhaltung fuer immer auf 0, obwohl sie eine der Spaetspiel-Geldsenken ist.
            // Der Client ruft das an derselben Stelle auf (Kontor.ZeigeZugnachrichten).
            SW.Dynamisch.VersuchTitelVerleihen(SW.Dynamisch.GetAktiverSpieler());

            // VersuchTitelVerleihen merkt den Titel nur vor; vollzogen wird er im Client durch den
            // TitelVerleihungManager in den Familienereignissen. Ohne diesen zweiten Schritt bliebe
            // der Rang stehen - und mit ihm die Hofhaltung bei 0.
            var verleihung = new Gameplay.Titel.TitelVerleihungManager();

            if (verleihung.StehtTitelverleihungAn())
                verleihung.Vollziehe();

            int vorStrafe = spieler.GetTaler();
            nachrichten.PruefeVerbrechen();
            zeile.Strafen = vorStrafe - spieler.GetTaler();

            if (_aufbau.Aemter)
                UebernimmAmt();

            if (_aufbau.Wahlen)
                BewirbDichUmAemter();

            zeile.Amtseinkommen = nachrichten.KassiereAmtseinkommen();
            nachrichten.AktualisiereAnwesen();

            if (_aufbau.Adel)
            {
                BaueWohnsitzAus();
                KaufeStuetzpunkt();
            }

            // 5. Wirtschaftliches Rundenende - Reihenfolge bindend, siehe Kontor.FuehreWirtschaftliches-
            //    RundenendeDurch: Das Reichtumswachstum liest die Verkaufsmengen, die RohBedarf danach
            //    verbraucht und nullt.
            SW.Dynamisch.RohPreiseRandomSchwanken();
            SW.Dynamisch.ReichtumWachstumAktRundenEnde();
            SW.Dynamisch.RohBedarfAktRundenEnde();
            SW.Dynamisch.EinwohnerWachstumAktRundenEnde();
            SW.Dynamisch.RundenBestechungenAbwickeln();

            if (_aufbau.Vollstaendig || _aufbau.Wahlen)
                SW.Dynamisch.KIAktionenDurchfuehren();

            // Der politische Block des Rundenendes, in der Reihenfolge von Kontor.ZeigeZugnachrichten:
            // erst die Amtsenthebungen, dann die Wahlen (solange die Kandidaten noch gemeldet sind),
            // zuletzt die Todesfaelle, die die Aemter des naechsten Jahres frei machen.
            if (_aufbau.Wahlen)
            {
                FuehreAmtsenthebungenDurch();
                HalteWahlenAb();
                new RundenEndeManager().FuehreKiTodesfaelleDurch();
            }

            _runden.SchalteZumNaechstenSpieler();

            zeile.Taler = spieler.GetTaler();
            zeile.Vermoegen = spieler.GetGesamtVermoegen(SpielerId);
            zeile.Abschlag = MittlererAbschlag();
            zeile.Titel = SW.Statisch.GetTitelX(spieler.GetTitel()).GetName(spieler.GetMaennlich());
            zeile.Amt = SW.Statisch.GetAmtwithID(spieler.GetAmtID()).GetAmtsname(spieler.GetMaennlich());
            zeile.Ansehen = spieler.GetAnsehen();
            zeile.StandesAnsehen = spieler.GetStandesAnsehen();

            PasseAuslastungAn(zeile);
            zeile.Auslastung = Auslastung;

            return zeile;
        }

        /// <summary>
        /// Beladet je Stadt die Karawane mit dem gelagerten Bestand und schickt sie in die Stadt, die
        /// derzeit am meisten zahlt.
        ///
        /// <c>SetzeVerkaufsAnzahl</c> nimmt die Ware sofort aus dem Lager - die Karawane wird beim
        /// Auftrag beladen, verkauft wird sie erst im Buch des naechsten Zuges, zum dann gueltigen
        /// Preis der Zielstadt.
        /// </summary>
        private void SetzeExportauftraege()
        {
            const int Verkaufsslot = 1;

            foreach (var (stadtId, _, rohstoffId) in _linien)
            {
                int bestand = SW.Dynamisch.GetAktHum().GetStadtRohstoffAnzahl(stadtId, rohstoffId);

                if (bestand <= 0)
                    continue;

                int ziel = BesteAbsatzstadt(stadtId, rohstoffId);

                if (ziel == 0)
                    continue;

                _handel.SetzeTaetigkeit(stadtId, Verkaufsslot, EnumProduktionsslotAktionsart.Verkaufen);
                _handel.SetzeVerkaufsRohstoff(stadtId, Verkaufsslot, rohstoffId);
                _handel.SetzeVerkaufsStadt(stadtId, Verkaufsslot, ziel);
                _handel.SetzeVerkaufsAnzahl(stadtId, Verkaufsslot, bestand);
            }
        }

        /// <summary>
        /// Die Stadt mit dem hoechsten aktuellen Preis fuer die Ware, die eigene ausgenommen. Das ist
        /// dieselbe Wahl, die der E2E-Treiber trifft - und weil der Preis den Saettigungsabschlag
        /// bereits enthaelt, weicht sie von selbst auf andere Maerkte aus, sobald einer voll ist.
        /// </summary>
        private static int BesteAbsatzstadt(int herkunft, int rohstoffId)
        {
            int beste = 0;
            int bester = -1;

            for (int stadtId = 1; stadtId < SW.Statisch.GetMaxStadtID(); stadtId++)
            {
                if (stadtId == herkunft)
                    continue;

                int preis = SW.Dynamisch.GetStadtwithID(stadtId).GetRohstoffPreisVonIDX(rohstoffId);

                if (preis > bester)
                {
                    bester = preis;
                    beste = stadtId;
                }
            }

            return beste;
        }

        /// <summary>
        /// Uebernimmt das eintraeglichste Amt, das der Rang erlaubt - und nur, wenn gerade keines
        /// gefuehrt wird. Ein Amt geht durch Amtsenthebung verloren; dann wird im naechsten Jahr das
        /// naechste genommen.
        /// </summary>
        private void UebernimmAmt()
        {
            var spieler = SW.Dynamisch.GetAktHum();

            // Auch mit Amt weitersuchen: Der Rang steigt im Lauf des Spiels, und mit ihm die
            // erlaubte Amtsebene. Wer beim ersten Stadtamt bleibt, verdient bis zum Ende 1 500
            // Taler im Jahr, obwohl ihm als Graf laengst Reichsaemter offenstuenden.
            int jetzigerErtrag = SW.Statisch.GetAmtwithID(spieler.GetAmtID()).GetEinkommen();

            int titel = spieler.GetTitel();
            int bestes = 0;
            int besterErtrag = 0;

            for (int amtId = 1; amtId < SW.Statisch.GetMaxAmtID(); amtId++)
            {
                if (!RangErlaubtAmt(titel, amtId))
                    continue;

                int ertrag = SW.Statisch.GetAmtwithID(amtId).GetEinkommen();

                if (ertrag > besterErtrag && ertrag > jetzigerErtrag)
                {
                    besterErtrag = ertrag;
                    bestes = amtId;
                }
            }

            if (bestes == 0)
                return;

            int stufe = SW.Dynamisch.GetStufeVonAmtmitIDx(bestes);

            // Gebiet 1 der Stufe; der Cheat erwartet den Gebietsindex 0-basiert und den Amtsindex
            // relativ zur Stufe.
            new CheatManager().UebernehmeAmt(stufe, 0, AmtsIndexInStufe(stufe, bestes));
        }

        /// <summary>
        /// Meldet den Spieler fuer jede Wahl an, die ihm offensteht. Welche das sind, entscheidet die
        /// Lib ueber <c>GetBewerbungsangebote</c> - dort steckt auch die Rangschranke. Gewinnt er ein
        /// hoeheres Amt, zieht ihn <c>FuelleRestlicheAemter</c> aus den niedrigeren Wahlen zurueck.
        /// </summary>
        private static void BewirbDichUmAemter()
        {
            var aemter = new AemterManager();

            foreach (var angebot in aemter.GetBewerbungsangebote())
            {
                if (!angebot.IstAngemeldet)
                    aemter.WahlAnmeldungUmschalten(angebot.WahlId);
            }
        }

        /// <summary>
        /// Wickelt die Amtsenthebungsverfahren ab. Ist der Spieler beteiligt, enthaelt er sich - der
        /// Client liesse ihn hier abstimmen oder sich verteidigen; eine Messung soll das Ergebnis nicht
        /// in die eine oder andere Richtung schieben.
        /// </summary>
        private static void FuehreAmtsenthebungenDurch()
        {
            var manager = new AmtsenthebungsManager();

            foreach (var verfahren in manager.ErmittleVerfahren())
            {
                if (verfahren.MenschlichBeteiligt)
                    manager.ErgaenzeKiStimmen(verfahren);

                manager.WerteAus(verfahren);
            }
        }

        /// <summary>
        /// Haelt die Wahlen ab - dieselbe Mechanik wie <c>WahlDialog.RunWahl</c>, nur ohne Anzeige:
        /// KI-Waehler stimmen ueber <c>ErmittleKiStimme</c>, menschliche Waehler stimmen fuer sich
        /// selbst, sofern sie kandidieren, sonst fuer den ersten Bewerber. <c>FuelleRestlicheAemter</c>
        /// zaehlt danach alles aus, was offen blieb.
        /// </summary>
        private static void HalteWahlenAb()
        {
            var aemter = new AemterManager();

            foreach (int wahlId in aemter.GetWahlenMitMenschlicherBeteiligung())
            {
                if (!aemter.HatMenschlicheBeteiligung(wahlId))
                    continue;

                var ansicht = aemter.ErstelleWahlAnsicht(wahlId);

                // Eine Wahl ohne Bewerber laesst sich nicht auszaehlen: WerteWahlAus zaehlt die
                // Stimmen direkt in das Kandidaten-Slotarray, jeder Index zeigt dann auf einen
                // leeren Platz, und GetSpWithID(0) ist null. Solche Wahlen entstehen, wenn der
                // Spieler nur Waehler ist und sich niemand beworben hat; FuelleRestlicheAemter
                // raeumt sie danach ab.
                if (ansicht.Kandidaten.Count == 0)
                    continue;

                if (ansicht.IstLoswahl)
                {
                    aemter.WerteWahlAus(wahlId, new List<int>());
                    continue;
                }

                var kandidatenIds = new List<int>();

                foreach (var kandidat in ansicht.Kandidaten)
                    kandidatenIds.Add(kandidat.SpielerId);

                // Gezaehlt wird in die verdichtete Kandidatenliste - dieselbe Reihenfolge, die
                // ErstelleWahlAnsicht liefert und auf die ErmittleKiStimme zeigt.
                var stimmen = new List<int>();

                foreach (var waehler in ansicht.Waehler)
                {
                    int stimme;

                    if (waehler.IstMensch)
                    {
                        // Fuer sich selbst, wenn man antritt - sonst fuer den ersten Bewerber.
                        int eigener = kandidatenIds.IndexOf(SpielerId);
                        stimme = eigener >= 0 ? eigener : 0;
                    }
                    else
                    {
                        stimme = aemter.ErmittleKiStimme(waehler.SpielerId, kandidatenIds);
                    }

                    stimmen.Add(stimme);
                }

                aemter.WerteWahlAus(wahlId, stimmen);
            }

            aemter.FuelleRestlicheAemter();
        }

        /// <summary>Ob der Rang fuer diese Amtsebene ausreicht.</summary>
        private static bool RangErlaubtAmt(int titel, int amtId)
        {
            if (amtId < SW.Statisch.GetMaxAmtStadtID())
                return titel >= SW.Statisch.GetMinTitelStadtEbene();

            if (amtId < SW.Statisch.GetMaxAmtLandID())
                return titel >= SW.Statisch.GetMinTitelLandEbene();

            return titel >= SW.Statisch.GetMinTitelReichsEbene();
        }

        /// <summary>
        /// Rechnet eine Amts-ID in den stufenrelativen Index um, den <c>CheatManager.UebernehmeAmt</c>
        /// erwartet - wie im E2E-Treiber.
        /// </summary>
        private static int AmtsIndexInStufe(int stufe, int amtId)
        {
            if (stufe == 1)
                return amtId - SW.Statisch.GetMaxAmtStadtID();

            if (stufe == 2)
                return amtId - SW.Statisch.GetMaxAmtLandID();

            return amtId;
        }

        /// <summary>Hoechste Wohnsitzstufe des Spiels (Schloss).</summary>
        private const int HoechsteHausstufe = 9;

        /// <summary>
        /// Baut den naechstbesseren Wohnsitz, sobald er bezahlbar ist - der erste Teil des Adelswegs.
        ///
        /// Seit der Korrektur der Wohnsitzbedingung genuegt ein Haus <b>mindestens</b> der verlangten
        /// Stufe, es reicht also, immer das beste bezahlbare zu bauen (Landherr braucht Stufe 5, der
        /// Herzog 9). Vorher verglich die Lib den Hausnamen exakt: Da musste je Rang genau das
        /// passende Haus stehen, und eine erste Fassung dieser Harness gab ueber 40 Jahre rund
        /// 925 000 Taler aus und blieb trotzdem Ritter.
        ///
        /// Je Stadt kann ein Haus stehen, gebaut wird deshalb ab Stadt 2 in der ersten freien - in
        /// Stadt 1 steht der Startwohnsitz. Die Haelfte der Barschaft bleibt als Puffer, und es laeuft
        /// hoechstens ein Bau zur Zeit.
        /// </summary>
        private void BaueWohnsitzAus()
        {
            var spieler = SW.Dynamisch.GetAktHum();
            var anwesen = new AnwesenManager();

            int beste = 0;

            for (int stufe = HoechsteHausstufe; stufe >= 1; stufe--)
            {
                if (spieler.BesitztSpielerFertigesHausAbStufe(stufe))
                {
                    beste = stufe;
                    break;
                }
            }

            if (beste >= HoechsteHausstufe)
                return;

            int budget = spieler.GetTaler() / 2;

            for (int stadtId = 2; stadtId < SW.Statisch.GetMaxStadtID(); stadtId++)
            {
                // Es wird schon irgendwo gebaut: abwarten, statt daneben einen zweiten Bau zu beginnen.
                if (anwesen.HatHaus(stadtId) && !anwesen.IstFertig(stadtId))
                    return;

                if (anwesen.HatHaus(stadtId))
                    continue;

                var angebote = anwesen.GetBaubareHaeuser(stadtId, 0);

                if (angebote == null)
                    continue;

                HausAngebot bestes = null;

                foreach (var angebot in angebote)
                {
                    if (angebot.HausId > beste && angebot.Preis <= budget &&
                        (bestes == null || angebot.HausId > bestes.HausId))
                    {
                        bestes = angebot;
                    }
                }

                if (bestes != null)
                    anwesen.BaueHaus(stadtId, bestes);

                return;
            }
        }

        /// <summary>
        /// Bietet einmal im Jahr auf einen Stützpunkt einer KI - der zweite Teil des Adelswegs.
        ///
        /// Graf und Fürst verlangen einen, der Herzog zwei; ohne Stützpunkte endet die Leiter beim
        /// Baron, und damit auch die Hofhaltung als Geldsenke.
        ///
        /// <c>KaufangebotAbgeben</c> haengt nicht nur am Geld: Angenommen wird nach Beziehung,
        /// Ansehen/10 und Religionssympathie, und nur der Betrag <b>ueber</b> dem geschätzten Wert
        /// zaehlt zusaetzlich (je angefangene 1 000 Taler ein Punkt). Deshalb wird das Doppelte des
        /// Werts geboten. Pro Jahr ist genau ein Angebot erlaubt; das Flag setzt
        /// <c>RundenManager.SchliesseZugAb</c> zurueck, den diese Harness ohnehin aufruft.
        ///
        /// Geboten wird nur auf Stützpunkte von KI-Spielern - fuer menschliche Besitzer nimmt
        /// <c>KaufangebotAbgeben</c> einen anderen Weg, den es in einer Einspieler-Messung nicht gibt.
        /// </summary>
        private void KaufeStuetzpunkt()
        {
            var spieler = SW.Dynamisch.GetAktHum();

            if (spieler.GetAnzahlStuetzpunkte(SpielerId) >= 2)
                return;

            var soeldner = new Gameplay.Kampf.SoeldnerRaeuberManager();

            for (int id = 1; id <= soeldner.Anzahl; id++)
            {
                int besitzer = soeldner.GetBesitzer(id);

                if (besitzer == SpielerId || besitzer < SW.Statisch.GetMinKIID())
                    continue;

                int gebot = SW.Dynamisch.GetStuetzpunkte()[id - 1].BerechneWert() * 2;

                if (gebot <= 0 || gebot > spieler.GetTaler() / 2)
                    continue;

                // Der Ja/Nein-Stub antwortet ohne Warten, der Aufruf laeuft also synchron durch.
                soeldner.KaufangebotAbgeben(id, gebot).GetAwaiter().GetResult();

                // Ob angenommen oder nicht: mehr als ein Angebot je Jahr geht nicht.
                return;
            }
        }

        /// <summary>
        /// Was eine Stadt im Jahr verbraucht, gemittelt ueber alle Staedte - die Menge, die ein Markt
        /// aufnimmt, ohne zu saettigen (<c>Stadt.GetRohstoffPreisVonIDX</c> rechnet den Abschlag in
        /// Jahresbedarfen, und ein Jahresbedarf ist Einwohner/10).
        /// </summary>
        private static int MittlererJahresbedarf()
        {
            int summe = 0;
            int anzahl = 0;

            for (int stadtId = 1; stadtId < SW.Statisch.GetMaxStadtID(); stadtId++)
            {
                summe += Math.Max(1, SW.Dynamisch.GetStadtwithID(stadtId).GetEinwohner() / 10);
                anzahl++;
            }

            return anzahl == 0 ? 1 : summe / anzahl;
        }

        /// <summary>Verkauft in jeder Stadt den dort gelagerten Bestand der eigenen Ware.</summary>
        private void VerkaufeBestaende(Jahreszeile zeile)
        {
            foreach (var (stadtId, _, rohstoffId) in _linien)
            {
                int bestand = SW.Dynamisch.GetAktHum().GetStadtRohstoffAnzahl(stadtId, rohstoffId);

                if (bestand <= 0)
                    continue;

                zeile.Erloes += _handel.VerkaufeRohstoff(stadtId, rohstoffId, bestand);
                zeile.Verkauft += bestand;
            }
        }

        /// <summary>
        /// Der mittlere Marktabschlag der belieferten Staedte - das Mass dafuer, wie sehr sich der
        /// Betrieb die eigenen Preise verdirbt.
        /// </summary>
        private int MittlererAbschlag()
        {
            if (_linien.Count == 0)
                return 0;

            int summe = 0;

            foreach (var (stadtId, _, rohstoffId) in _linien)
            {
                var stadt = SW.Dynamisch.GetStadtwithID(stadtId);
                int jahresbedarf = Math.Max(1, stadt.GetEinwohner() / 10);
                int roh = stadt.GetRohstoffIDXVorrat(rohstoffId) * Gameplay.Gebiete.Stadt.AbschlagJeBedarfsjahrProzent / jahresbedarf;

                summe += Math.Min(Gameplay.Gebiete.Stadt.MaxAbschlagProzent, roh);
            }

            return summe / _linien.Count;
        }

        #endregion
    }
}
