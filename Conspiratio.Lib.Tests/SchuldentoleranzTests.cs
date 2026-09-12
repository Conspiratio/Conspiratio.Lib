using Conspiratio.Lib.Allgemein;
using Conspiratio.Lib.Gameplay.Niederlassung;
using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>
    /// Ab welchem Minus ein Spieler sich vor den Gläubigern verantworten muss (Spielrunden-Feedback).
    ///
    /// Die Schwelle lag bei festen −500 Talern, unabhängig davon, wie groß der Betrieb war, den sie
    /// beurteilte. Schon eine einzelne Werkstatt kostet rund 5 000 Taler im Jahr: Ein einziges
    /// Verlustjahr brachte damit vor Gericht, und der Schuldturm kostet den Zug und damit das
    /// Einkommen, mit dem die Schuld zu tilgen wäre. Jetzt ist die Toleranz der Jahresbetriebskosten-
    /// block – ein schlechtes Jahr ist ein Rückschlag, zwei sind ein Prozess.
    /// </summary>
    public class SchuldentoleranzTests
    {
        /// <summary>Richtet einen produzierenden Platz ein und liefert dessen Jahresbetriebskosten.</summary>
        private static int RichteBetriebEin(int staetten, int arbeiter)
        {
            var spieler = SW.Dynamisch.GetAktHum();
            var slot = spieler.GetProduktionsslot(1, 0);

            slot.SetTaetigkeit((int)EnumProduktionsslotAktionsart.Produzieren);
            slot.SetProduktionRohstoff(1);
            slot.SetProduktionStaetten(staetten);
            slot.SetProduktionArbeiter(arbeiter);

            return AbrechnungsManager.BerechneJahresBetriebskosten(spieler);
        }

        [Fact]
        public void Ohne_Betrieb_bleibt_es_beim_alten_Boden_von_500_Talern()
        {
            TestSpielwelt.Starte();
            var spieler = SW.Dynamisch.GetAktHum();

            spieler.SetTaler(-499);
            Assert.False(new ZugNachrichtenManager().MussSichVorGlaeubigernVerantworten());

            spieler.SetTaler(-501);
            Assert.True(new ZugNachrichtenManager().MussSichVorGlaeubigernVerantworten());
        }

        /// <summary>
        /// Der Kern der Änderung: Wer einen Betrieb führt, darf so tief ins Minus, wie dieser Betrieb
        /// im Jahr kostet. Vorher war bei −500 Schluss, gleichgültig wie groß er war.
        /// </summary>
        [Fact]
        public void Mit_Betrieb_reicht_die_Toleranz_bis_zu_dessen_Jahreskosten()
        {
            TestSpielwelt.Starte();
            var spieler = SW.Dynamisch.GetAktHum();
            int kosten = RichteBetriebEin(staetten: 6, arbeiter: 42);

            Assert.True(kosten > 500, "Der Testbetrieb muss teurer sein als der alte Boden, sonst prüft der Test nichts.");

            // Knapp innerhalb der Kosten: noch kein Prozess – vorher wäre das längst einer gewesen.
            spieler.SetTaler(-(kosten - 1));
            Assert.False(new ZugNachrichtenManager().MussSichVorGlaeubigernVerantworten());

            // Darüber hinaus schon.
            spieler.SetTaler(-(kosten + 1));
            Assert.True(new ZugNachrichtenManager().MussSichVorGlaeubigernVerantworten());
        }

        [Fact]
        public void Ein_groesserer_Betrieb_hat_mehr_Luft_als_ein_kleiner()
        {
            TestSpielwelt.Starte();

            int kleiner = RichteBetriebEin(staetten: 2, arbeiter: 14);
            int grosser = RichteBetriebEin(staetten: 8, arbeiter: 56);

            Assert.True(grosser > kleiner);
        }

        /// <summary>
        /// Die 100-Taler-Untergrenze bleibt: Ein kleines Minus ist nie ein Fall für die Gläubiger,
        /// auch wenn ein Betrieb ohne Arbeiter und Stätten gar keine Toleranz erzeugte.
        /// </summary>
        [Fact]
        public void Ein_kleines_Minus_bleibt_folgenlos()
        {
            TestSpielwelt.Starte();
            SW.Dynamisch.GetAktHum().SetTaler(-99);

            Assert.False(new ZugNachrichtenManager().MussSichVorGlaeubigernVerantworten());
        }

        /// <summary>
        /// Ansehen verschafft weiterhin zusätzliche Nachsicht – der Zuschlag wirkt auf die neue
        /// Schwelle genauso wie auf die alte.
        /// </summary>
        [Fact]
        public void Ansehen_verschafft_zusaetzliche_Nachsicht()
        {
            TestSpielwelt.Starte();
            var spieler = SW.Dynamisch.GetAktHum();
            int kosten = RichteBetriebEin(staetten: 6, arbeiter: 42);

            spieler.SetTaler(-(kosten + 100));
            Assert.True(new ZugNachrichtenManager().MussSichVorGlaeubigernVerantworten());

            spieler.SetAnsehen(spieler.GetAnsehen() + 50);
            Assert.False(new ZugNachrichtenManager().MussSichVorGlaeubigernVerantworten());
        }
    }
}
