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
        public int Hofhaltung;
        public int Gesamtkosten;
        public int Strafen;

        /// <summary>Mittlerer Marktabschlag der belieferten Staedte in Prozentpunkten.</summary>
        public int Abschlag;

        /// <summary>Erreichter Rang - ohne ihn ist die Hofhaltung nicht zu deuten.</summary>
        public string Titel;

        /// <summary>Der simulierte Kapazitaetsunterhalt dieses Jahres (siehe Aufbau).</summary>
        public int Kapazitaet;
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
        /// Simuliert einen <b>vorgeschlagenen</b>, noch nicht umgesetzten Kapazitaetsunterhalt: Die
        /// n-te Produktionsstaette einer Linie kostet das n-Fache dieser Grundeinheit im Jahr, der
        /// Betrag wird nach der Jahresabrechnung abgezogen. 0 schaltet ihn ab.
        ///
        /// <b>Die Lib bleibt dafuer unveraendert.</b> Der heutige Unterhalt ist zwar schon progressiv,
        /// bemisst sich aber am <i>Besitz an Werkstaetten</i> (<c>ZaehleWerkstaetten</c>), und genau
        /// daran waechst ein Haendler nicht: Gemessen sind 14 Werkstaetten, aber 646 Produktionsstaetten
        /// und 1 360 Arbeiter - Unterhalt 10 500 Taler. Erloes und Kosten steigen beide linear mit der
        /// Menge, die Marge bleibt konstant, und das Vermoegen waechst ewig geradeaus. Dieser Schalter
        /// misst, wie stark eine Progression auf der <i>Kapazitaet</i> sein muesste, damit die Kurve
        /// abflacht.
        /// </summary>
        public int KapazitaetsunterhaltJeStaette;
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

            // Erst nach dem Aufbau setzen: Die Spielerstellung zieht selbst Zufallszahlen, und ein Seed
            // davor wuerde vom Setup verbraucht statt von den gemessenen Jahren.
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
                    int rohstoffId = SW.Dynamisch.GetStadtwithID(stadtId).GetSingleRohstoff(platz);

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

                if (ausLager < ausArbeitern)
                    lagerBindet++;

                _handel.SetzeProduktionsStaetten(stadtId, slot, staetten);
                _handel.SetzeProduktionsArbeiter(stadtId, slot, staetten * arbeiterProStaette);

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
            zeile.Hofhaltung = abrechnung.Hofhaltung;
            zeile.Gesamtkosten = abrechnung.Gesamtkosten;

            zeile.Kapazitaet = ZieheKapazitaetsunterhaltAb();

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

            nachrichten.KassiereAmtseinkommen();
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

            if (_aufbau.Vollstaendig)
                SW.Dynamisch.KIAktionenDurchfuehren();

            _runden.SchalteZumNaechstenSpieler();

            zeile.Taler = spieler.GetTaler();
            zeile.Vermoegen = spieler.GetGesamtVermoegen(SpielerId);
            zeile.Abschlag = MittlererAbschlag();
            zeile.Titel = SW.Statisch.GetTitelX(spieler.GetTitel()).GetName(spieler.GetMaennlich());

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
        /// Zieht den simulierten Kapazitaetsunterhalt ab: je Linie die Dreieckszahl ihrer Staetten mal
        /// der Grundeinheit. Zwei Staetten kosten also drei Einheiten, 46 Staetten 1 081 - die
        /// Progression trifft den Magnaten und laesst den Einsteiger in Ruhe.
        /// </summary>
        private int ZieheKapazitaetsunterhaltAb()
        {
            if (_aufbau.KapazitaetsunterhaltJeStaette <= 0)
                return 0;

            // Progression ueber die GESAMTE Kapazitaet, nicht je Linie: Die n-te Staette des Betriebs
            // kostet das n-Fache eines Tausendstels der Grundeinheit. Je Linie gerechnet war die
            // Dreieckszahl zu steil - 46 Staetten je Linie zu quadrieren traf haerter als die 14
            // Werkstaetten, an denen der heutige Unterhalt haengt.
            long staetten = 0;

            foreach (var (stadtId, slot, _) in _linien)
                staetten += _handel.GetProduktionsslot(stadtId, slot).GetProduktionStaetten();

            int summe = (int)(staetten * (staetten + 1) / 2 * _aufbau.KapazitaetsunterhaltJeStaette / 1000);

            SW.Dynamisch.GetAktHum().ErhoeheTaler(-summe);

            return summe;
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
