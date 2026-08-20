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

            Assert.Equal(50, SW.Dynamisch.Spielstand.Einstellungen.KiAggressivitaetProzent);
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
    }
}
