using Conspiratio.Lib.Allgemein;
using Conspiratio.Lib.Gameplay.Niederlassung;
using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>
    /// Der Kapazitätsunterhalt: die n-te betriebene Produktionsstätte kostet das n-Fache eines
    /// Tausendstels von `KapazitaetsunterhaltProMille`.
    ///
    /// Er tritt neben den Werkstattunterhalt, der am **Besitz** hängt. Beide zusammen halten die Regel
    /// aufrecht, die im Kommentar an `AbrechnungsErgebnis.Unterhalt` steht: Horten ist nicht gratis
    /// (Werkstattunterhalt), und wer groß produziert, zahlt dafür (Kapazitätsunterhalt).
    ///
    /// Gebraucht wird er, weil ein Händler nicht an der Zahl seiner Werkstätten wächst, sondern an den
    /// Stätten darin: gemessen 14 Werkstätten, aber 646 Stätten — und ein Vermögen, das 40 Jahre lang
    /// geradeaus stieg.
    /// </summary>
    public class KapazitaetsunterhaltTests
    {
        private const int Korn = 1;

        /// <summary>
        /// Verteilt <paramref name="staetten"/> Produktionsstätten über die Slots und meldet die
        /// tatsächlich gesetzte Summe. Gesetzt wird direkt am Slot: `HandelsManager` würde auf den
        /// Werkstattbesitz klemmen, und geprüft wird hier die Abrechnung, nicht die Eingabe.
        /// </summary>
        private static int VerteileStaetten(int staetten)
        {
            var spieler = SW.Dynamisch.GetAktHum();
            int verteilt = 0;

            for (int stadtId = 1; stadtId < SW.Statisch.GetMaxStadtID() && verteilt < staetten; stadtId++)
            {
                for (int slot = 0; slot < SW.Statisch.GetMaxProdSlots() && verteilt < staetten; slot++)
                {
                    int anzahl = System.Math.Min(50, staetten - verteilt);
                    var produktionsslot = spieler.GetProduktionsslot(stadtId, slot);

                    produktionsslot.SetTaetigkeit((int)EnumProduktionsslotAktionsart.Produzieren);
                    produktionsslot.SetProduktionRohstoff(Korn);
                    produktionsslot.SetProduktionStaetten(anzahl);

                    verteilt += anzahl;
                }
            }

            return verteilt;
        }

        private static int Erwartet(int staetten)
            => staetten * (staetten + 1) / 2 * AbrechnungsManager.KapazitaetsunterhaltProMille / 1000;

        [Fact]
        public void Er_waechst_quadratisch_mit_der_Staettenzahl()
        {
            TestSpielwelt.Starte();
            int staetten = VerteileStaetten(100);

            var ergebnis = new AbrechnungsManager().ErstelleAbrechnungFuerAktivenSpieler();

            Assert.Equal(Erwartet(staetten), ergebnis.Kapazitaetsunterhalt);
        }

        /// <summary>
        /// Der Einsteiger merkt nichts: Zwei Stätten kosten 3 × 140 / 1000, ganzzahlig also nichts. Das
        /// ist die Bedingung, unter der die Abgabe als Spätspiel-Bremse überhaupt taugt.
        /// </summary>
        [Fact]
        public void Ein_kleiner_Betrieb_zahlt_nichts()
        {
            TestSpielwelt.Starte();
            VerteileStaetten(2);

            var ergebnis = new AbrechnungsManager().ErstelleAbrechnungFuerAktivenSpieler();

            Assert.Equal(0, ergebnis.Kapazitaetsunterhalt);
        }

        /// <summary>
        /// Doppelte Kapazität kostet ungefähr das Vierfache — das ist der Unterschied zu den linearen
        /// Betriebskosten und der Grund, aus dem die Abgabe die Kurve überhaupt abflachen kann.
        /// </summary>
        [Fact]
        public void Doppelte_Kapazitaet_kostet_rund_das_Vierfache()
        {
            TestSpielwelt.Starte();
            int wenig = VerteileStaetten(100);
            int fuerWenig = new AbrechnungsManager().ErstelleAbrechnungFuerAktivenSpieler().Kapazitaetsunterhalt;

            TestSpielwelt.Starte();
            int viel = VerteileStaetten(2 * wenig);
            int fuerViel = new AbrechnungsManager().ErstelleAbrechnungFuerAktivenSpieler().Kapazitaetsunterhalt;

            Assert.Equal(2 * wenig, viel);
            Assert.InRange(fuerViel, 3.8 * fuerWenig, 4.2 * fuerWenig);
        }

        [Fact]
        public void Er_steckt_in_den_Gesamtkosten()
        {
            TestSpielwelt.Starte();
            VerteileStaetten(100);

            var ergebnis = new AbrechnungsManager().ErstelleAbrechnungFuerAktivenSpieler();

            Assert.True(ergebnis.Kapazitaetsunterhalt > 0, "100 Staetten muessen etwas kosten.");
            Assert.True(ergebnis.Gesamtkosten >= ergebnis.Kapazitaetsunterhalt + ergebnis.Unterhalt,
                        "Beide Unterhaltsarten muessen in den Gesamtkosten stecken.");
        }

        /// <summary>
        /// Nur betriebene Stätten zählen. Ein Slot im Verkaufsmodus produziert nichts, also kostet er
        /// auch keinen Kapazitätsunterhalt — der Werkstattunterhalt trifft den Besitz weiterhin.
        /// </summary>
        [Fact]
        public void Ein_Verkaufsslot_kostet_keinen_Kapazitaetsunterhalt()
        {
            TestSpielwelt.Starte();
            VerteileStaetten(100);

            var spieler = SW.Dynamisch.GetAktHum();

            for (int stadtId = 1; stadtId < SW.Statisch.GetMaxStadtID(); stadtId++)
                for (int slot = 0; slot < SW.Statisch.GetMaxProdSlots(); slot++)
                    spieler.GetProduktionsslot(stadtId, slot)
                           .SetTaetigkeit((int)EnumProduktionsslotAktionsart.Verkaufen);

            var ergebnis = new AbrechnungsManager().ErstelleAbrechnungFuerAktivenSpieler();

            Assert.Equal(0, ergebnis.Kapazitaetsunterhalt);
        }
    }
}
