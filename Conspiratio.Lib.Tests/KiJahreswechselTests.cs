using Conspiratio.Lib.Allgemein;
using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>
    /// Der Jahreswechsel der KI-Spieler (`DynamischeSpieldaten.KIAktionenDurchfuehren`).
    ///
    /// Diese Tests halten die Kette fest, an der ein fehlender Aufruf im Godot-Client hing: Ohne Altern
    /// sinken die verbleibenden Lebensjahre nicht, ohne das greift die Sterbeformel nicht, ohne Todesfälle
    /// wird kein Amt frei – und ohne freie Ämter gibt es weder Wahlen noch Handelszertifikate.
    /// </summary>
    public class KiJahreswechselTests
    {
        private const int Jahre = 15;

        /// <summary>
        /// Der Nachweis der Ursache. Ohne das Altern sinken die verbleibenden Lebensjahre nie, sodass
        /// nur jene wenigen KIs sterben, die schon bei Spielbeginn am Ende ihrer Zeit stehen – gemessen
        /// 13 gegenüber 163 in {Jahre} Jahren, ein Zwölftel. Für den Spieler heißt das: In seiner Stadt
        /// wird über ein ganzes Spiel hinweg mit großer Wahrscheinlichkeit kein einziges Amt frei.
        /// Verglichen wird das Verhältnis, nicht der absolute Wert – der hängt am Startwert.
        /// </summary>
        [Fact]
        public void Das_Altern_vervielfacht_die_Zahl_der_Todesfaelle()
        {
            TestSpielwelt.Starte(seed: 1234);
            int ohneJahreswechsel = SpieleJahre(mitJahreswechsel: false);

            TestSpielwelt.Starte(seed: 1234);
            int mitJahreswechsel = SpieleJahre(mitJahreswechsel: true);

            Assert.True(mitJahreswechsel > ohneJahreswechsel * 5,
                        $"Mit Jahreswechsel starben {mitJahreswechsel}, ohne {ohneJahreswechsel} KI-Spieler");
        }

        [Fact]
        public void Mit_Jahreswechsel_sterben_KIs_regelmaessig()
        {
            TestSpielwelt.Starte(seed: 1234);

            int tode = SpieleJahre(mitJahreswechsel: true);

            // Gemessen 163 bei diesem Startwert; die Schranke lässt reichlich Luft nach unten.
            Assert.True(tode > 100, $"Ueber {Jahre} Jahre starben nur {tode} KI-Spieler");
        }

        [Fact]
        public void Mit_Jahreswechsel_werden_Aemter_frei()
        {
            TestSpielwelt.Starte(seed: 1234);

            int besetztVorher = ZaehleBesetzteAemter();
            SpieleJahre(mitJahreswechsel: true);
            int besetztNachher = ZaehleBesetzteAemter();

            Assert.True(besetztNachher < besetztVorher,
                        $"Es wurde kein Amt frei ({besetztVorher} vorher, {besetztNachher} nachher)");
        }

        [Fact]
        public void Der_Jahreswechsel_laesst_die_KIs_altern()
        {
            TestSpielwelt.Starte(seed: 1234);

            int ersteKi = SW.Statisch.GetMinKIID();
            int alterVorher = SW.Dynamisch.GetSpWithID(ersteKi).GetAlter();

            SW.Dynamisch.KIAktionenDurchfuehren();

            Assert.Equal(alterVorher + 1, SW.Dynamisch.GetSpWithID(ersteKi).GetAlter());
        }

        /// <summary>Spielt <see cref="Jahre"/> Rundenenden durch und liefert die Zahl der Todesfälle.</summary>
        private static int SpieleJahre(bool mitJahreswechsel)
        {
            var rundenEnde = new RundenEndeManager();
            int tode = 0;

            for (int jahr = 0; jahr < Jahre; jahr++)
            {
                if (mitJahreswechsel)
                    SW.Dynamisch.KIAktionenDurchfuehren();

                tode += rundenEnde.FuehreKiTodesfaelleDurch().Count;
            }

            return tode;
        }

        private static int ZaehleBesetzteAemter()
        {
            int besetzt = 0;

            for (int i = SW.Statisch.GetMinKIID(); i < SW.Statisch.GetMaxKIID(); i++)
                if (SW.Dynamisch.GetSpWithID(i).GetAmtID() != 0)
                    besetzt++;

            return besetzt;
        }
    }
}
