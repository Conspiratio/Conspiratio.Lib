using Conspiratio.Lib.Allgemein;
using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>Laufende Wirkung einer Sabotage, die eine KI gegen den Menschen laufen hat.</summary>
    public class GegnerischeSabotageTests
    {
        [Fact]
        public void Ohne_laufende_Sabotage_passiert_nichts()
        {
            TestSpielwelt.Starte();
            Assert.Null(new ZugNachrichtenManager().ErmittleGegnerischeSabotageNachrichten());
        }

        [Fact]
        public void Schaden_trifft_ungefaehr_jedes_zweite_Jahr_und_zehrt_die_Dauer_auf()
        {
            TestSpielwelt.Starte(seed: 7);
            int kiId = TestSpielwelt.SetzeKiGegner(0, 0);
            var mensch = SW.Dynamisch.GetAktHum();
            var manager = new ZugNachrichtenManager();

            int treffer = 0;
            const int versuche = 1000;

            for (int i = 0; i < versuche; i++)
            {
                mensch.GetGegnerischeSabotage(kiId).SetDauer(1);
                string meldung = manager.ErmittleGegnerischeSabotageNachrichten();

                if (meldung != null)
                {
                    treffer++;
                    Assert.Equal(0, mensch.GetGegnerischeSabotage(kiId).GetDauer());
                }
            }

            double rate = (double)treffer / versuche;
            Assert.InRange(rate, 0.40, 0.60); // Chance 1/2, grosse Stichprobe statt Einzelfall
        }

        [Fact]
        public void Verteidigungsprivileg_19_senkt_die_Trefferchance_auf_ein_Viertel()
        {
            TestSpielwelt.Starte(seed: 7);
            int kiId = TestSpielwelt.SetzeKiGegner(0, 0);
            var mensch = SW.Dynamisch.GetAktHum();
            mensch.SetPrivilegX(19, true);
            var manager = new ZugNachrichtenManager();

            int treffer = 0;
            const int versuche = 2000;

            for (int i = 0; i < versuche; i++)
            {
                mensch.GetGegnerischeSabotage(kiId).SetDauer(1);
                if (manager.ErmittleGegnerischeSabotageNachrichten() != null)
                    treffer++;
            }

            double rate = (double)treffer / versuche;
            Assert.InRange(rate, 0.17, 0.33); // Chance 1/4
        }

        [Fact]
        public void Schaden_wird_vom_Vermoegen_des_Menschen_abgezogen()
        {
            TestSpielwelt.Starte(seed: 7);
            int kiId = TestSpielwelt.SetzeKiGegner(0, 0);
            var mensch = SW.Dynamisch.GetAktHum();
            var manager = new ZugNachrichtenManager();

            int talerVorher = mensch.GetTaler();
            string meldung = null;

            for (int i = 0; i < 200 && meldung == null; i++)
            {
                mensch.GetGegnerischeSabotage(kiId).SetDauer(1);
                meldung = manager.ErmittleGegnerischeSabotageNachrichten();
            }

            Assert.NotNull(meldung);
            Assert.True(mensch.GetTaler() < talerVorher);
        }
    }
}
