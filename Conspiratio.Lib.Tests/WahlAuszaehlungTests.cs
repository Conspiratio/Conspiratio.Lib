using System.Collections.Generic;

using Conspiratio.Lib.Allgemein;
using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>
    /// Die Auszählung einer Wahl. `AemterManager.WerteWahlAus` zählt die Stimmen in die **verdichtete**
    /// Kandidatenliste – dieselbe Reihenfolge, die `ErstelleWahlAnsicht` liefert und auf die auch
    /// `ErmittleKiStimme` zeigt.
    ///
    /// Früher wurde direkt in das rohe Slotarray gezählt. Beide Zählweisen stimmen nur überein, solange
    /// die Plätze lückenlos von vorn belegt sind; sind sie es nicht, ging die Stimme an den falschen
    /// Bewerber oder auf einen leeren Platz – und dort scheiterte `GetSpWithID(0)`. Der Godot-Wahldialog
    /// baut seine Indizes aus der verdichteten Ansicht und war damit unmittelbar betroffen.
    /// </summary>
    public class WahlAuszaehlungTests
    {
        /// <summary>Die Wahlen sind 1-basiert - Wahlen[0] bleibt null (DynamischeSpieldaten).</summary>
        private const int Wahlnummer = 1;
        private const int AmtRichter = 5;
        private const int StadtGebiet = 1;
        private const int StufeStadt = 0;

        private const int Mensch = 1;

        /// <summary>Legt eine Stadtwahl an und setzt die Kandidaten genau auf die genannten Plätze.</summary>
        private static void BereiteWahlVor(params (int Platz, int SpielerId)[] kandidaten)
        {
            TestSpielwelt.Starte();

            var wahl = SW.Dynamisch.GetWahlX(Wahlnummer);
            wahl.NullSetzen();
            wahl.AmtID = AmtRichter;
            wahl.GebietID = StadtGebiet;
            wahl.Stufe = StufeStadt;

            foreach (var (platz, spielerId) in kandidaten)
                wahl.SetKandidatenXAufY(platz, spielerId);
        }

        private static int ErsteKi() => SW.Statisch.GetMinKIID();

        private static int ZweiteKi() => SW.Statisch.GetMinKIID() + 1;

        [Fact]
        public void Ohne_Luecke_gewinnt_der_gewaehlte_Bewerber()
        {
            BereiteWahlVor((0, ErsteKi()), (1, Mensch));

            var ergebnis = new AemterManager().WerteWahlAus(Wahlnummer, new List<int> { 1, 1 });

            Assert.Equal(Mensch, ergebnis.GewinnerId);
        }

        /// <summary>
        /// Der eigentliche Fehlerfall: Platz 1 ist leer, der zweite Bewerber steht auf Platz 2. Eine
        /// Stimme auf den verdichteten Index 1 muss ihn treffen. Vorher zeigte sie auf den leeren Platz.
        /// </summary>
        [Fact]
        public void Mit_Luecke_gewinnt_trotzdem_der_gewaehlte_Bewerber()
        {
            BereiteWahlVor((0, ErsteKi()), (2, Mensch));

            var ergebnis = new AemterManager().WerteWahlAus(Wahlnummer, new List<int> { 1, 1 });

            Assert.Equal(Mensch, ergebnis.GewinnerId);
        }

        /// <summary>Und in die andere Richtung: Index 0 trifft weiterhin den ersten Bewerber.</summary>
        [Fact]
        public void Mit_Luecke_trifft_Index_null_den_ersten_Bewerber()
        {
            BereiteWahlVor((0, ErsteKi()), (2, Mensch));

            var ergebnis = new AemterManager().WerteWahlAus(Wahlnummer, new List<int> { 0, 0 });

            Assert.Equal(ErsteKi(), ergebnis.GewinnerId);
        }

        /// <summary>
        /// Auch wenn der erste Platz frei ist: Die Verdichtung beginnt beim ersten belegten Platz, ein
        /// leerer Platz gewinnt nie.
        /// </summary>
        [Fact]
        public void Ein_leerer_Platz_gewinnt_nie()
        {
            BereiteWahlVor((1, ErsteKi()), (3, Mensch));

            var ergebnis = new AemterManager().WerteWahlAus(Wahlnummer, new List<int> { 0 });

            Assert.Equal(ErsteKi(), ergebnis.GewinnerId);
            Assert.NotEqual(0, ergebnis.GewinnerId);
        }

        /// <summary>
        /// Eine Wahl ohne Bewerber darf nicht abstuerzen. Sie vergibt nichts; FuelleRestlicheAemter
        /// raeumt sie danach ab.
        /// </summary>
        [Fact]
        public void Ohne_Bewerber_wird_nichts_vergeben()
        {
            BereiteWahlVor();

            var ergebnis = new AemterManager().WerteWahlAus(Wahlnummer, new List<int> { 0 });

            Assert.Equal(0, ergebnis.GewinnerId);
        }

        /// <summary>
        /// Die Loswahl ohne Waehler zieht ebenfalls nur unter den belegten Plaetzen - mit einer Luecke
        /// im Feld darf sie keinen leeren Platz ziehen.
        /// </summary>
        [Fact]
        public void Die_Loswahl_zieht_nur_unter_Bewerbern()
        {
            for (int versuch = 0; versuch < 20; versuch++)
            {
                BereiteWahlVor((0, ErsteKi()), (2, Mensch), (4, ZweiteKi()));

                var ergebnis = new AemterManager().WerteWahlAus(Wahlnummer, new List<int>());

                Assert.Contains(ergebnis.GewinnerId, new[] { ErsteKi(), Mensch, ZweiteKi() });
                Assert.True(ergebnis.WarLoswahl);
            }
        }
    }
}
