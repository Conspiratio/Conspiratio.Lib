using Conspiratio.Lib.Allgemein;
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

        private const int Korn = 1;

        /// <summary>
        /// Trägt einen Jahresabsatz in der Stadt ein, so wie ihn <c>VerkaufeRohstoff</c> hinterlässt –
        /// ohne den vollen Handelsweg nachzuspielen.
        /// </summary>
        private static void SetzeJahresabsatz(int stadtId, int menge)
        {
            SW.Dynamisch.GetAktHum().SetEinVerkaeufeInStadtXVonRohstoffIDYAufZ(stadtId, Korn, menge);
        }

        [Fact]
        public void Ohne_Handel_steigt_der_Reichtum_nicht()
        {
            TestSpielwelt.Starte(seed: 1);

            var stadt = SW.Dynamisch.GetStadtwithID(GrosseStadt);
            SetzeJahresabsatz(GrosseStadt, 0);
            stadt.SetReichtumToX(4);

            for (int runde = 0; runde < 20; runde++)
                SW.Dynamisch.ReichtumWachstumAktRundenEnde();

            Assert.Equal(4, stadt.GetReichtum());
        }

        [Fact]
        public void Reger_Handel_macht_die_Stadt_reicher()
        {
            TestSpielwelt.Starte(seed: 1);

            var stadt = SW.Dynamisch.GetStadtwithID(GrosseStadt);
            stadt.SetReichtumToX(3);
            stadt.SetKriminalitaetAufX(1);

            // Der Wurf ist probabilistisch, deshalb über mehrere Runden prüfen statt über eine.
            for (int runde = 0; runde < 20; runde++)
            {
                SetzeJahresabsatz(GrosseStadt, 2400);
                SW.Dynamisch.ReichtumWachstumAktRundenEnde();
            }

            Assert.True(stadt.GetReichtum() > 3,
                        "Zwanzig Jahre regen Handels müssen den Reichtum der Stadt heben.");
        }

        [Fact]
        public void Kriminalitaet_bremst_den_Reichtumszuwachs()
        {
            TestSpielwelt.Starte(seed: 1);

            var sicher = SW.Dynamisch.GetStadtwithID(GrosseStadt);
            var unsicher = SW.Dynamisch.GetStadtwithID(KleineStadt);

            sicher.SetReichtumToX(3);
            unsicher.SetReichtumToX(3);
            sicher.SetKriminalitaetAufX(1);
            unsicher.SetKriminalitaetAufX(5);

            // Der Wurf ist probabilistisch (20 % gegen 5 % Jahreschance), deshalb eine lange Messreihe:
            // über wenige Runden entscheidet das Rauschen, nicht der Erwartungswert.
            for (int runde = 0; runde < 100; runde++)
            {
                SetzeJahresabsatz(GrosseStadt, 2000);
                SetzeJahresabsatz(KleineStadt, 2000);
                SW.Dynamisch.ReichtumWachstumAktRundenEnde();
            }

            Assert.True(sicher.GetReichtum() > unsicher.GetReichtum(),
                        "Bei gleichem Handel muss die sichere Stadt stärker profitieren (sicher: " +
                        sicher.GetReichtum() + ", unsicher: " + unsicher.GetReichtum() + ").");
        }

        [Fact]
        public void Der_Reichtum_ueberschreitet_die_Obergrenze_nie()
        {
            TestSpielwelt.Starte(seed: 1);

            var stadt = SW.Dynamisch.GetStadtwithID(GrosseStadt);
            stadt.SetReichtumToX(SW.Statisch.GetMaxReichtum());
            stadt.SetKriminalitaetAufX(1);

            for (int runde = 0; runde < 40; runde++)
            {
                SetzeJahresabsatz(GrosseStadt, 100000);
                SW.Dynamisch.ReichtumWachstumAktRundenEnde();
            }

            // SetReichtumToX klemmt nicht von sich aus - die Methode muss es tun.
            Assert.Equal(SW.Statisch.GetMaxReichtum(), stadt.GetReichtum());
        }

        /// <summary>
        /// Ergänzt den Fall, den der Test darüber nicht trifft: Er startet <b>am</b> Limit und prüft damit
        /// nur den Frühausstieg. Hier steigt der Reichtum tatsächlich noch <b>auf</b> das Limit und bleibt
        /// dann stehen – das ist die Grenze, an der ein Abbruch um eins danebenliegen könnte.
        /// </summary>
        [Fact]
        public void Der_Reichtum_steigt_genau_bis_auf_die_Obergrenze()
        {
            TestSpielwelt.Starte(seed: 1);

            var stadt = SW.Dynamisch.GetStadtwithID(GrosseStadt);
            stadt.SetReichtumToX(SW.Statisch.GetMaxReichtum() - 1);
            stadt.SetKriminalitaetAufX(1);

            for (int runde = 0; runde < 60; runde++)
            {
                SetzeJahresabsatz(GrosseStadt, 100000);
                SW.Dynamisch.ReichtumWachstumAktRundenEnde();
            }

            Assert.Equal(SW.Statisch.GetMaxReichtum(), stadt.GetReichtum());
        }

        /// <summary>
        /// Hält die bindende Aufrufreihenfolge fest: <c>RohBedarfAktRundenEnde</c> nullt die
        /// Verkaufsmengen. Liefe es zuerst, misst das Reichtumswachstum dauerhaft ein Volumen von null
        /// und wäre wirkungslos. Dieser Test schlägt an, falls die Reihenfolge je vertauscht wird.
        /// </summary>
        [Fact]
        public void Nach_der_Vorratsbuchung_ist_das_Handelsvolumen_verbraucht()
        {
            TestSpielwelt.Starte(seed: 1);

            var stadt = SW.Dynamisch.GetStadtwithID(GrosseStadt);
            stadt.SetReichtumToX(3);
            stadt.SetKriminalitaetAufX(1);

            for (int runde = 0; runde < 20; runde++)
            {
                SetzeJahresabsatz(GrosseStadt, 2400);
                SW.Dynamisch.RohBedarfAktRundenEnde();      // verbraucht und nullt die Menge
                SW.Dynamisch.ReichtumWachstumAktRundenEnde();
            }

            Assert.Equal(3, stadt.GetReichtum());
        }
    }
}
