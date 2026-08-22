using System.Linq;

using Conspiratio.Lib.Allgemein;
using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>
    /// Die jährliche Gesetzgebung der KI-Minister. Sie lief im Godot-Client bislang überhaupt nicht –
    /// die Gesetzeslage stand über ein ganzes Spiel hinweg still.
    /// </summary>
    public class KiGesetzgebungTests
    {
        [Fact]
        public void Ueber_viele_Jahre_aendern_die_KI_Minister_alle_drei_Ressorts()
        {
            TestSpielwelt.Starte(seed: 2023);

            var manager = new KiGesetzgebungManager();
            var ressorts = new System.Collections.Generic.HashSet<string>();

            for (int jahr = 0; jahr < 300; jahr++)
                foreach (var meldung in manager.FuehreGesetzesaenderungenDurch())
                    ressorts.Add(meldung.Ressort);

            Assert.Contains("Finanzgesetze", ressorts);
            Assert.Contains("Strafgesetze", ressorts);
            Assert.Contains("Kirchengesetze", ressorts);
        }

        [Fact]
        public void Ein_Ressort_wird_nur_selten_angefasst()
        {
            TestSpielwelt.Starte(seed: 2023);

            var manager = new KiGesetzgebungManager();
            const int jahre = 3000;
            int finanzaenderungen = 0;

            for (int jahr = 0; jahr < jahre; jahr++)
                finanzaenderungen += manager.FuehreGesetzesaenderungenDurch().Count(m => m.Ressort == "Finanzgesetze");

            // Im Original: Rnd.Next(1, 8) == 1, also rund ein Siebtel der Jahre.
            Assert.InRange((double)finanzaenderungen / jahre, 0.11, 0.18);
        }

        [Fact]
        public void Die_Meldung_nennt_den_Minister_und_die_neue_Gesetzeslage()
        {
            TestSpielwelt.Starte(seed: 2023);

            var manager = new KiGesetzgebungManager();
            GesetzesaenderungsMeldung meldung = null;

            for (int jahr = 0; jahr < 300 && meldung == null; jahr++)
                meldung = manager.FuehreGesetzesaenderungenDurch().FirstOrDefault();

            Assert.NotNull(meldung);
            Assert.Contains("ändert die", meldung.Text);
            Assert.NotEmpty(meldung.Ressort);
        }

        [Fact]
        public void Ein_menschlicher_Minister_aendert_seine_Gesetze_selbst()
        {
            TestSpielwelt.Starte(seed: 2023);

            // Der Spieler wird Finanzminister – dann darf ihm niemand mehr hineinregieren.
            const int AmtFinanzminister = 38;  // Reich.SetAmtXtoY
            SW.Dynamisch.GetReichWithID(1).SetAmtXtoY(AmtFinanzminister, SW.Dynamisch.GetAktiverSpieler());

            var manager = new KiGesetzgebungManager();

            for (int jahr = 0; jahr < 300; jahr++)
                Assert.DoesNotContain(manager.FuehreGesetzesaenderungenDurch(), m => m.Ressort == "Finanzgesetze");
        }
    }
}
