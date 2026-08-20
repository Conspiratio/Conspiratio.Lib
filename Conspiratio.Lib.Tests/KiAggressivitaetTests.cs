using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>
    /// Die eine Einstellung für die Aggressivität der KI-Spieler (0–100 %). 50 % muss überall
    /// exakt das bisherige Verhalten reproduzieren.
    /// </summary>
    public class KiAggressivitaetTests
    {
        [Fact]
        public void Der_Standardwert_ist_fuenfzig_Prozent()
        {
            TestSpielwelt.Starte();

            // Bewusst nur über den Accessor geprüft: Der Spielstand entsteht über einen Pfad, der
            // Feldinitialisierer umgeht (FormatterServices.GetUninitializedObject, siehe CLAUDE.md),
            // die rohe Property darf dort also 0 sein. Genau dafür gibt es den Fallback – auf den
            // Rohwert zu assertieren würde der Prämisse dieses Features widersprechen.
            Assert.Equal(50, SW.Dynamisch.GetKiAggressivitaetProzent());
        }

        [Theory]
        [InlineData(0, 50)]    // Alter Spielstand ohne das Feld -> wie 50 %
        [InlineData(-5, 50)]   // Defensiv: negative Werte ebenso
        [InlineData(1, 1)]
        [InlineData(50, 50)]
        [InlineData(100, 100)]
        public void Nicht_gesetzte_Werte_gelten_als_fuenfzig_Prozent(int gesetzt, int erwartet)
        {
            TestSpielwelt.Starte();
            SW.Dynamisch.Spielstand.Einstellungen.KiAggressivitaetProzent = gesetzt;

            Assert.Equal(erwartet, SW.Dynamisch.GetKiAggressivitaetProzent());
        }

        [Theory]
        [InlineData(50, 60, 60)]    // Standard: unveraendert
        [InlineData(100, 60, 100)]  // +50, an der Obergrenze gekappt
        [InlineData(100, 10, 60)]   // +50
        [InlineData(1, 60, 11)]     // -49 (1 % ist der untere Anschlag, 0 hiesse "nicht gesetzt")
        [InlineData(1, 10, 0)]      // -49, an der Untergrenze gekappt
        [InlineData(75, 20, 45)]    // +25, stufenlos dazwischen
        public void Die_Bosheit_wird_um_die_Aggressivitaet_verschoben(int prozent, int roh, int erwartet)
        {
            TestSpielwelt.Starte();
            int kiId = TestSpielwelt.SetzeKiGegner(0, 0, bosheit: roh);
            SW.Dynamisch.Spielstand.Einstellungen.KiAggressivitaetProzent = prozent;

            Assert.Equal(erwartet, SW.Dynamisch.GetKIwithID(kiId).GetBosheit());
        }

        [Fact]
        public void Die_Einstellung_wirkt_sofort_auf_bestehende_KIs()
        {
            // Kernzusage des Features: Der Regler wirkt rueckwirkend, ohne neues Spiel.
            TestSpielwelt.Starte();
            int kiId = TestSpielwelt.SetzeKiGegner(0, 0, bosheit: 40);
            var ki = SW.Dynamisch.GetKIwithID(kiId);

            SW.Dynamisch.Spielstand.Einstellungen.KiAggressivitaetProzent = 50;
            int vorher = ki.GetBosheit();

            SW.Dynamisch.Spielstand.Einstellungen.KiAggressivitaetProzent = 90;
            int nachher = ki.GetBosheit();

            Assert.Equal(40, vorher);
            Assert.Equal(80, nachher);
        }

        [Fact]
        public void Der_gespeicherte_Charakterwert_bleibt_unveraendert()
        {
            // _boese wird serialisiert; nur die Auswirkung wird moduliert, nicht der Charakter selbst.
            TestSpielwelt.Starte();
            int kiId = TestSpielwelt.SetzeKiGegner(0, 0, bosheit: 30);
            var ki = SW.Dynamisch.GetKIwithID(kiId);

            SW.Dynamisch.Spielstand.Einstellungen.KiAggressivitaetProzent = 100;
            Assert.Equal(80, ki.GetBosheit());

            // Zurueckdrehen liefert exakt den Ausgangswert - der Rohwert wurde nie ueberschrieben.
            SW.Dynamisch.Spielstand.Einstellungen.KiAggressivitaetProzent = 50;
            Assert.Equal(30, ki.GetBosheit());
        }

        [Fact]
        public void Die_Streuung_zwischen_den_KIs_bleibt_erhalten()
        {
            TestSpielwelt.Starte();
            TestSpielwelt.SetzeKiGegner(0, 0, bosheit: 10);
            TestSpielwelt.SetzeKiGegner(1, 0, bosheit: 40);
            SW.Dynamisch.Spielstand.Einstellungen.KiAggressivitaetProzent = 80;

            int ersteKi = SW.Statisch.GetMinKIID();
            int zweiteKi = ersteKi + 1;

            // Beide um +30 verschoben, der Abstand von 30 Punkten bleibt: die Einstellung
            // verschiebt gemeinsam, sie gleicht die Charaktere nicht an.
            Assert.Equal(40, SW.Dynamisch.GetKIwithID(ersteKi).GetBosheit());
            Assert.Equal(70, SW.Dynamisch.GetKIwithID(zweiteKi).GetBosheit());
        }

        [Theory]
        [InlineData(1, -2)]     // frueher "Niedrig" (1 % ist der untere Anschlag): -2 + 7*1/50 = -2
        [InlineData(50, 5)]     // frueher "Mittel" - der Regressionsanker
        [InlineData(100, 10)]   // frueher "Hoch"
        [InlineData(25, 1)]     // stufenlos: -2 + (5-(-2))*25/50 = 1,5 -> 1 (Ganzzahl)
        [InlineData(75, 7)]     // stufenlos: 5 + (10-5)*25/50 = 7,5 -> 7
        public void Die_Interpolation_trifft_die_alten_Stufen_und_dazwischen(int prozent, int erwartet)
        {
            TestSpielwelt.Starte();
            SW.Dynamisch.Spielstand.Einstellungen.KiAggressivitaetProzent = prozent;

            // Wertetripel des Gerichts-Urteilsfaktors (frueher Niedrig/Mittel/Hoch = -2/+5/+10).
            Assert.Equal(erwartet, SW.Dynamisch.InterpoliereNachAggressivitaet(-2, 5, 10));
        }

        [Fact]
        public void Die_Interpolation_ist_bei_fuenfzig_Prozent_exakt_der_Mittelwert()
        {
            // Gilt fuer alle drei realen Wertetripel: bei 50 % aendert sich gegenueber heute nichts.
            TestSpielwelt.Starte();
            SW.Dynamisch.Spielstand.Einstellungen.KiAggressivitaetProzent = 50;

            Assert.Equal(5, SW.Dynamisch.InterpoliereNachAggressivitaet(-2, 5, 10));    // Urteilsfaktor
            Assert.Equal(10, SW.Dynamisch.InterpoliereNachAggressivitaet(-2, 10, 20));  // Absetz-Sympathie
            Assert.Equal(5, SW.Dynamisch.InterpoliereNachAggressivitaet(2, 5, 12));     // Anklage-Faktor
        }

        [Fact]
        public void Die_Interpolation_steigt_monoton()
        {
            TestSpielwelt.Starte();
            int vorheriger = int.MinValue;

            for (int prozent = 1; prozent <= 100; prozent++)
            {
                SW.Dynamisch.Spielstand.Einstellungen.KiAggressivitaetProzent = prozent;
                int wert = SW.Dynamisch.InterpoliereNachAggressivitaet(2, 5, 12);

                Assert.True(wert >= vorheriger, $"Bei {prozent} % sank der Wert von {vorheriger} auf {wert}");
                vorheriger = wert;
            }
        }
    }
}
