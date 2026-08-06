using System.Collections.Generic;

using Conspiratio.Lib.Gameplay.Spielwelt;

using JetBrains.Annotations;

namespace Conspiratio.Lib.Allgemein
{
    /// <summary>Eine Beleidigung und der Konter, der sie mit Wortwitz aufgreift.</summary>
    public class SpruchPaar
    {
        public string Beleidigung { get; set; }
        public string Konter { get; set; }
    }

    /// <summary>Erster Schritt eines Wortwechsels: Der Angreifer wählt eine Beleidigung.</summary>
    public class AngriffsRunde
    {
        /// <summary>Wählt der aktive (menschliche) Spieler, oder sein Gegner?</summary>
        public bool WaehlerIstAktiverSpieler { get; set; }

        /// <summary>Muss ein Mensch klicken? Bei false wählt die KI über <see cref="WortgefechtManager.WaehleKiAngriff"/>.</summary>
        public bool MenschWaehlt { get; set; }

        /// <summary>Name dessen, der gerade wählt (für den Hot-Seat-Hinweis).</summary>
        public string WaehlerName { get; set; }

        public IReadOnlyList<string> Optionen { get; set; }
    }

    /// <summary>Zweiter Schritt eines Wortwechsels: Der Angegriffene sucht den passenden Konter.</summary>
    public class KonterRunde
    {
        public bool WaehlerIstAktiverSpieler { get; set; }

        /// <summary>Muss ein Mensch klicken? Bei false wählt die KI über <see cref="WortgefechtManager.WaehleKiKonter"/>.</summary>
        public bool MenschWaehlt { get; set; }

        public string WaehlerName { get; set; }

        /// <summary>Die Beleidigung, auf die geantwortet werden muss.</summary>
        public string Beleidigung { get; set; }

        public IReadOnlyList<string> Optionen { get; set; }

        /// <summary>Index des Konters, der tatsächlich zur Beleidigung passt.</summary>
        public int RichtigerIndex { get; set; }
    }

    /// <summary>Ausgang eines Wortwechsels.</summary>
    public class RundenErgebnis
    {
        /// <summary>Geht der Treffer an den aktiven Spieler?</summary>
        public bool SpielerPunktet { get; set; }

        public bool KonterWarRichtig { get; set; }

        /// <summary>Der tatsächlich gewählte Konter (auch wenn er unpassend war).</summary>
        public string Erwiderung { get; set; }

        /// <summary>Erzählerkommentar zum Ausgang des Wortwechsels.</summary>
        public string Kommentar { get; set; }
    }

    /// <summary>
    /// Das Wortgefecht eines Duells (Issue #17, Vorbild „Monkey Island 1"): Statt den Sieger zu würfeln,
    /// wird das Duell als Schlagabtausch von Beleidigungen gespielt. Ein Wortwechsel besteht aus zwei
    /// Schritten – der Angreifer wählt eine Beleidigung, der Angegriffene muss den passenden Konter
    /// erkennen. Wer den passenden Konter findet, macht den Treffer; sonst sitzt die Beleidigung.
    /// Wer zuerst <see cref="Trefferziel"/> Wortwechsel gewinnt, gewinnt das Duell.
    ///
    /// Die Fechtfähigkeit wirkt hybrid: Sie bringt einen (gedeckelten) Treffervorsprung und lässt die
    /// eigenen Beleidigungen gegen KI-Gegner häufiger sitzen. Gegen einen menschlichen Gegner wählen
    /// beide Seiten selbst – dann entscheidet allein das Erkennen der passenden Konter.
    ///
    /// Gemessene Balance gegen einen mittleren Gegner (Bosheit 50): Wer immer den passenden Konter
    /// findet, gewinnt ohne jeden Fechtunterricht rund 70 % der Duelle und mit Fechtfähigkeit 150
    /// praktisch alle. Wer dagegen nie den passenden Konter findet, gewinnt selbst mit maximalem
    /// Fechtunterricht nur rund 40 % – Training hilft spürbar, ersetzt den Witz aber nicht.
    /// </summary>
    public class WortgefechtManager
    {
        /// <summary>Gewonnene Wortwechsel, die zum Sieg nötig sind (Best of 5).</summary>
        public const int Trefferziel = 3;

        /// <summary>Wie viele Sprüche jeweils zur Auswahl stehen (mindestens 3).</summary>
        public const int AnzahlOptionen = 3;

        /// <summary>Fechtfähigkeits-Differenz je Treffer Vorsprung.</summary>
        private const int VorsprungTeiler = 25;

        /// <summary>Maximaler Vorsprung, damit ein Duell nie vorentschieden ist.</summary>
        private const int MaxVorsprung = 1;

        // Wie oft die KI beim Kontern den passenden Spruch findet. Bewusst gedämpft (halbes Gewicht,
        // enger Deckel): Der Spieler kann nur in den Runden eingreifen, in denen er selbst kontert –
        // stünde die KI in den übrigen Runden zu gut da, wäre perfektes Spiel ohne Fechtunterricht
        // chancenlos. So gewinnt Können die Konterrunden und die Fechtfähigkeit die Angriffsrunden.
        private const int KiKonterBasis = 40;
        private const int KiKonterGewicht = 2;
        private const int KiKonterMin = 35;
        private const int KiKonterMax = 70;

        /// <summary>
        /// Beleidigung und passender Konter. Die falschen Antwortmöglichkeiten sind die Konter der
        /// anderen Paare – die klingen dann absurd und sind damit als unpassend zu erkennen.
        /// </summary>
        private static readonly SpruchPaar[] Sprueche =
        {
            new SpruchPaar { Beleidigung = "Ihr kämpft wie ein Bauerntölpel!",
                             Konter      = "Wie passend – Ihr riecht auch wie einer!" },
            new SpruchPaar { Beleidigung = "Meine Großmutter ficht besser als Ihr!",
                             Konter      = "Dann holt sie her, mit ihr plaudere ich lieber!" },
            new SpruchPaar { Beleidigung = "Ihr habt das Fechten wohl aus einem Kochbuch gelernt!",
                             Konter      = "Und Ihr Euren Witz aus dem Misthaufen!" },
            new SpruchPaar { Beleidigung = "Eure Klinge ist so stumpf wie Euer Verstand!",
                             Konter      = "Zum Glück genügt beides vollauf für Euch!" },
            new SpruchPaar { Beleidigung = "Nie sah ich einen so feigen Wicht!",
                             Konter      = "Dann schaut in den Spiegel, dort steht einer!" },
            new SpruchPaar { Beleidigung = "Ihr stolpert über Euren eigenen Schatten!",
                             Konter      = "Immerhin habe ich einen – Ihr seid zu blass dafür!" },
            new SpruchPaar { Beleidigung = "Euer Wappen hat wohl ein Kind gemalt!",
                             Konter      = "Es hat Euch gemalt – darum ist es so hässlich!" },
            new SpruchPaar { Beleidigung = "Ihr habt den Anstand eines Schweinehirten!",
                             Konter      = "Und Ihr die Manieren seiner Schweine!" },
            new SpruchPaar { Beleidigung = "Euer Atem setzt mir härter zu als Eure Klinge!",
                             Konter      = "Dann haltet Abstand – Euch wird beides zum Verhängnis!" },
            new SpruchPaar { Beleidigung = "Ihr zittert ja wie Espenlaub!",
                             Konter      = "Vor Lachen, nicht vor Furcht!" },
            new SpruchPaar { Beleidigung = "Man nennt Euch den Schrecken der Schankmägde!",
                             Konter      = "Und Euch den Schrecken jeder Zahnlücke!" },
            new SpruchPaar { Beleidigung = "Für einen Amtsträger fechtet Ihr erbärmlich!",
                             Konter      = "Für einen Nichtsnutz redet Ihr erstaunlich viel!" }
        };

        private readonly int _zielId;
        private readonly bool _gegnerIstMensch;
        private readonly int _gegnerStaerke;
        private readonly int _fechtfaehigkeit;
        private readonly string _spielerName;
        private readonly string _gegnerName;

        /// <summary>Noch nicht verwendete Spruchpaare – kein Paar wiederholt sich im selben Duell.</summary>
        private readonly List<SpruchPaar> _vorrat = new List<SpruchPaar>();

        /// <summary>Die zur Wahl gestellten Paare des laufenden Angriffs.</summary>
        private readonly List<SpruchPaar> _angriffsKandidaten = new List<SpruchPaar>();

        private SpruchPaar _aktuellesPaar;
        private KonterRunde _offeneKonterRunde;

        /// <summary>Greift im laufenden Wortwechsel der aktive Spieler an? Die Rolle wechselt danach.</summary>
        private bool _spielerGreiftAn = true;

        public WortgefechtManager(int zielId)
        {
            _zielId = zielId;

            var spieler = SW.Dynamisch.GetAktHum();
            _fechtfaehigkeit = spieler.Fechtfaehigkeit;
            _spielerName = spieler.GetName();

            var ziel = SW.Dynamisch.GetSpWithID(zielId);
            _gegnerName = ziel.GetName();
            _gegnerIstMensch = zielId < SW.Statisch.GetMinKIID();

            _gegnerStaerke = _gegnerIstMensch
                ? SW.Dynamisch.GetHumWithID(zielId).Fechtfaehigkeit
                : 15 + SW.Dynamisch.GetKIwithID(zielId).GetBosheit() / 4;

            // Hybrid: Die Fechtfähigkeit bringt einen gedeckelten Treffervorsprung.
            int vorsprung = (_fechtfaehigkeit - _gegnerStaerke) / VorsprungTeiler;
            if (vorsprung > MaxVorsprung) vorsprung = MaxVorsprung;
            if (vorsprung < -MaxVorsprung) vorsprung = -MaxVorsprung;

            if (vorsprung > 0)
                SpielerTreffer = vorsprung;
            else if (vorsprung < 0)
                GegnerTreffer = -vorsprung;

            FuelleVorrat();
        }

        public int SpielerTreffer { get; private set; }

        public int GegnerTreffer { get; private set; }

        public bool IstBeendet => SpielerTreffer >= Trefferziel || GegnerTreffer >= Trefferziel;

        public bool SpielerHatGewonnen => SpielerTreffer >= Trefferziel;

        /// <summary>Ist der Gegner ein menschlicher Mitspieler (Hot-Seat)?</summary>
        [PublicAPI]
        public bool GegnerIstMensch => _gegnerIstMensch;

        [PublicAPI]
        public string GegnerName => _gegnerName;

        /// <summary>Erster Schritt: Wer angreift, wählt aus mehreren Beleidigungen.</summary>
        [PublicAPI]
        public AngriffsRunde NaechsterAngriff()
        {
            if (_vorrat.Count < AnzahlOptionen)
                FuelleVorrat();

            _angriffsKandidaten.Clear();
            var optionen = new List<string>();

            // Ohne Zurücklegen ziehen, damit keine Beleidigung doppelt zur Wahl steht.
            var topf = new List<SpruchPaar>(_vorrat);
            for (int i = 0; i < AnzahlOptionen; i++)
            {
                int index = SW.Statisch.Rnd.Next(0, topf.Count);
                _angriffsKandidaten.Add(topf[index]);
                optionen.Add(topf[index].Beleidigung);
                topf.RemoveAt(index);
            }

            return new AngriffsRunde
            {
                WaehlerIstAktiverSpieler = _spielerGreiftAn,
                MenschWaehlt = _spielerGreiftAn || _gegnerIstMensch,
                WaehlerName = _spielerGreiftAn ? _spielerName : _gegnerName,
                Optionen = optionen
            };
        }

        /// <summary>Zufällige Beleidigung der KI (nur wenn <see cref="AngriffsRunde.MenschWaehlt"/> false ist).</summary>
        [PublicAPI]
        public int WaehleKiAngriff() => SW.Statisch.Rnd.Next(0, _angriffsKandidaten.Count);

        /// <summary>
        /// Zweiter Schritt: Die gewählte Beleidigung steht fest, der Angegriffene sucht den passenden
        /// Konter. Genau eine Antwort passt, die übrigen gehören zu anderen Beleidigungen.
        /// </summary>
        [PublicAPI]
        public KonterRunde WaehleAngriff(int index)
        {
            _aktuellesPaar = _angriffsKandidaten[index];
            _vorrat.Remove(_aktuellesPaar);

            // Falsche Konter aus anderen Paaren ziehen.
            var falsche = new List<SpruchPaar>();
            foreach (var paar in Sprueche)
            {
                if (paar != _aktuellesPaar)
                    falsche.Add(paar);
            }

            var optionen = new List<string> { _aktuellesPaar.Konter };
            for (int i = 0; i < AnzahlOptionen - 1; i++)
            {
                int auswahl = SW.Statisch.Rnd.Next(0, falsche.Count);
                optionen.Add(falsche[auswahl].Konter);
                falsche.RemoveAt(auswahl);
            }

            // Mischen (Fisher-Yates), danach den richtigen Konter wiederfinden.
            for (int i = optionen.Count - 1; i > 0; i--)
            {
                int j = SW.Statisch.Rnd.Next(0, i + 1);
                string zwischen = optionen[i];
                optionen[i] = optionen[j];
                optionen[j] = zwischen;
            }

            bool verteidigerIstSpieler = !_spielerGreiftAn;

            _offeneKonterRunde = new KonterRunde
            {
                WaehlerIstAktiverSpieler = verteidigerIstSpieler,
                MenschWaehlt = verteidigerIstSpieler || _gegnerIstMensch,
                WaehlerName = verteidigerIstSpieler ? _spielerName : _gegnerName,
                Beleidigung = _aktuellesPaar.Beleidigung,
                Optionen = optionen,
                RichtigerIndex = optionen.IndexOf(_aktuellesPaar.Konter)
            };

            return _offeneKonterRunde;
        }

        /// <summary>
        /// Konter der KI (nur wenn <see cref="KonterRunde.MenschWaehlt"/> false ist): Ob sie den passenden
        /// Spruch findet, hängt von ihrer Stärke gegenüber der Fechtfähigkeit des Spielers ab.
        /// </summary>
        [PublicAPI]
        public int WaehleKiKonter()
        {
            int chance = KiKonterBasis + (_gegnerStaerke - _fechtfaehigkeit) / KiKonterGewicht;
            if (chance < KiKonterMin) chance = KiKonterMin;
            if (chance > KiKonterMax) chance = KiKonterMax;

            if (SW.Statisch.Rnd.Next(0, 100) < chance)
                return _offeneKonterRunde.RichtigerIndex;

            // Danebengreifen: einen der unpassenden Konter wählen.
            int falsch = SW.Statisch.Rnd.Next(0, _offeneKonterRunde.Optionen.Count - 1);
            return falsch >= _offeneKonterRunde.RichtigerIndex ? falsch + 1 : falsch;
        }

        /// <summary>Wertet den gewählten Konter aus und vergibt den Treffer.</summary>
        [PublicAPI]
        public RundenErgebnis WerteKonterAus(int index)
        {
            bool richtig = index == _offeneKonterRunde.RichtigerIndex;
            bool verteidigerIstSpieler = _offeneKonterRunde.WaehlerIstAktiverSpieler;

            // Der passende Konter bringt dem Verteidiger den Treffer, sonst sitzt die Beleidigung.
            bool spielerPunktet = richtig == verteidigerIstSpieler;

            if (spielerPunktet)
                SpielerTreffer++;
            else
                GegnerTreffer++;

            var ergebnis = new RundenErgebnis
            {
                SpielerPunktet = spielerPunktet,
                KonterWarRichtig = richtig,
                Erwiderung = _offeneKonterRunde.Optionen[index],
                Kommentar = BaueKommentar(richtig, verteidigerIstSpieler)
            };

            // Im nächsten Wortwechsel greift die andere Seite an.
            _spielerGreiftAn = !_spielerGreiftAn;

            return ergebnis;
        }

        /// <summary>
        /// Würfelt den Ausgang wie im nicht interaktiven Duell – für Clients bzw. Spieler, die das
        /// Wortgefecht nicht selbst austragen wollen.
        /// </summary>
        [PublicAPI]
        public bool WuerfleAusgang()
        {
            var ausgang = SW.Statisch.Rnd.Next(0, 100) < new FechtDuellManager().BerechneSiegchance(_zielId);

            if (ausgang)
                SpielerTreffer = Trefferziel;
            else
                GegnerTreffer = Trefferziel;

            return ausgang;
        }

        private string BaueKommentar(bool richtig, bool verteidigerIstSpieler)
        {
            if (richtig)
            {
                return verteidigerIstSpieler
                    ? "Die Erwiderung sitzt – " + _gegnerName + " weicht zurück!"
                    : _gegnerName + " pariert Euren Spott mühelos.";
            }

            return verteidigerIstSpieler
                ? "Die Antwort geht ins Leere – der Spott trifft Euch!"
                : "Die Erwiderung passt nicht – Euer Spott sitzt!";
        }

        private void FuelleVorrat()
        {
            _vorrat.Clear();
            foreach (var paar in Sprueche)
                _vorrat.Add(paar);
        }
    }
}
