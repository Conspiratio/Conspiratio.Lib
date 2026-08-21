using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>
    /// Bevölkerungswachstum: Bisher konnte die Einwohnerzahl nur durch Katastrophen sinken. Ohne
    /// Gegengewicht schrumpfen alle Städte über die Spieldauer, was den Warenabsatz dauerhaft
    /// unrentabel machen würde – der Jahresbedarf (<c>Einwohner / 10</c>) ist die Bezugsgröße des
    /// Mengenabschlags.
    /// </summary>
    public class BevoelkerungTests
    {
        private const int GrosseStadt = 1;
        private const int KleineStadt = 2;

        [Fact]
        public void Eine_Stadt_waechst_ueber_die_Runden()
        {
            TestSpielwelt.Starte(seed: 1);

            var stadt = SW.Dynamisch.GetStadtwithID(GrosseStadt);
            int vorher = stadt.GetEinwohner();

            for (int runde = 0; runde < 10; runde++)
                SW.Dynamisch.EinwohnerWachstumAktRundenEnde();

            Assert.True(stadt.GetEinwohner() > vorher,
                        "Nach zehn Runden muss die Stadt gewachsen sein, sonst fehlt das Gegengewicht zu den Katastrophen.");
        }

        [Fact]
        public void Reichtum_beschleunigt_und_Kriminalitaet_bremst_das_Wachstum()
        {
            TestSpielwelt.Starte(seed: 1);

            var reich = SW.Dynamisch.GetStadtwithID(GrosseStadt);
            var arm = SW.Dynamisch.GetStadtwithID(KleineStadt);

            // Gleiche Ausgangsgröße, damit nur die Standortfaktoren den Unterschied machen.
            reich.SetEinwohnerAufX(4000);
            arm.SetEinwohnerAufX(4000);
            reich.SetReichtumToX(7);
            reich.SetKriminalitaetAufX(1);
            arm.SetReichtumToX(1);
            arm.SetKriminalitaetAufX(5);

            for (int runde = 0; runde < 25; runde++)
                SW.Dynamisch.EinwohnerWachstumAktRundenEnde();

            Assert.True(reich.GetEinwohner() > arm.GetEinwohner(),
                        "Reichtum zieht Menschen an, Kriminalität vertreibt sie – über 25 Runden muss sich das zeigen.");
        }

        [Fact]
        public void Das_Wachstum_ueberschreitet_die_Obergrenze_nie()
        {
            TestSpielwelt.Starte(seed: 1);

            var stadt = SW.Dynamisch.GetStadtwithID(GrosseStadt);
            stadt.SetEinwohnerAufX(DynamischeSpieldaten.MaxEinwohner);

            for (int runde = 0; runde < 50; runde++)
                SW.Dynamisch.EinwohnerWachstumAktRundenEnde();

            Assert.Equal(DynamischeSpieldaten.MaxEinwohner, stadt.GetEinwohner());
        }

        /// <summary>
        /// Multiplikatives Wachstum käme aus der Null nie heraus (0 × irgendetwas = 0). Die Untergrenze
        /// macht eine von Katastrophen verwüstete Stadt wieder erholbar.
        /// </summary>
        [Fact]
        public void Eine_entvoelkerte_Stadt_erholt_sich()
        {
            TestSpielwelt.Starte(seed: 1);

            var stadt = SW.Dynamisch.GetStadtwithID(GrosseStadt);
            stadt.SetEinwohnerAufX(0);

            SW.Dynamisch.EinwohnerWachstumAktRundenEnde();

            Assert.Equal(DynamischeSpieldaten.MindestEinwohner, stadt.GetEinwohner());
        }
    }
}
