using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>
    /// Die Wohnsitzbedingung der Titelleiter. Sie prüft die <b>Stufe</b> des Hauses, nicht seinen Namen:
    /// Landherr verlangt Stufe 5 (Landhaus), Freiherr 6 (Villa), Baron 7 (Landsitz), Graf 8 (Burg),
    /// Fürst und Herzog 9 (Schloss) – bei ebenso steigenden Talerschwellen.
    ///
    /// Vorher wurde der Hausname exakt verglichen. Das machte den teureren Kauf schlechter: Wer statt
    /// eines Landhauses (10 000 Taler) ein Schloss (250 000) baute, verlor den Landherrn-Titel, den er
    /// sonst bekommen hätte.
    /// </summary>
    public class TitelWohnsitzTests
    {
        private const int SpielerId = 1;
        private const int Stadt = 1;

        private const int Huette = 2;
        private const int Landhaus = 5;
        private const int Villa = 6;
        private const int Schloss = 9;

        private const int TitelLandherr = 4;
        private const int TitelHerzog = 9;

        /// <summary>Setzt dem Spieler ein fertiges Haus der gewünschten Stufe in die Stadt.</summary>
        private static void GibHaus(int hausId, int restlicheBauzeit = 0)
        {
            var haus = SW.Dynamisch.GetAktHum().GetSpielerHatHausVonStadtAnArraystelle(Stadt);
            haus.SetHausID(hausId);
            haus.SetRestlicheBauzeit(restlicheBauzeit);
        }

        [Fact]
        public void Das_verlangte_Haus_genuegt()
        {
            TestSpielwelt.Starte();
            GibHaus(Landhaus);

            Assert.True(SW.Dynamisch.GetAktHum().BesitztSpielerFertigesHausAbStufe(Landhaus));
        }

        /// <summary>
        /// Der eigentliche Fehlerfall: Das teuerste Haus des Spiels erfüllt die Bedingung des Landherrn.
        /// Mit dem früheren Namensvergleich tat es das nicht.
        /// </summary>
        [Fact]
        public void Ein_hoeheres_Haus_genuegt_ebenfalls()
        {
            TestSpielwelt.Starte();
            GibHaus(Schloss);

            Assert.True(SW.Dynamisch.GetAktHum().BesitztSpielerFertigesHausAbStufe(Landhaus));
            Assert.True(SW.Dynamisch.GetAktHum().BesitztSpielerFertigesHausAbStufe(Villa));
        }

        [Fact]
        public void Ein_niedrigeres_Haus_genuegt_nicht()
        {
            TestSpielwelt.Starte();
            GibHaus(Huette);

            Assert.False(SW.Dynamisch.GetAktHum().BesitztSpielerFertigesHausAbStufe(Landhaus));
        }

        [Fact]
        public void Ein_unfertiges_Haus_zaehlt_nicht()
        {
            TestSpielwelt.Starte();
            GibHaus(Schloss, restlicheBauzeit: 3);

            Assert.False(SW.Dynamisch.GetAktHum().BesitztSpielerFertigesHausAbStufe(Landhaus));
        }

        /// <summary>
        /// Ein Schloss und genug Taler machen aus dem Spieler einen Landherrn. Das ist die Regel, wie
        /// der Spieler sie erlebt – über <c>IstSpielerFuerTitelBerechtigt</c> statt über den Helfer.
        /// </summary>
        [Fact]
        public void Mit_Schloss_und_Talern_ist_der_Landherr_faellig()
        {
            TestSpielwelt.Starte();
            GibHaus(Schloss);

            var landherr = SW.Statisch.GetTitelX(TitelLandherr);
            SW.Dynamisch.GetAktHum().SetTaler(landherr.AbTaler);

            Assert.True(landherr.IstSpielerFuerTitelBerechtigt(SW.Dynamisch, SpielerId));
        }

        /// <summary>
        /// Die Leiter bleibt eine Leiter: Ohne Stützpunkte reicht auch das Schloss nur bis zum Baron,
        /// Graf und höher verlangen zusätzlich Stützpunkte. Ohne diese Zusicherung wäre der
        /// Stufenvergleich ein Freifahrtschein bis zum Herzog.
        /// </summary>
        [Fact]
        public void Ohne_Stuetzpunkte_bleibt_der_Herzog_verwehrt()
        {
            TestSpielwelt.Starte();
            GibHaus(Schloss);

            var herzog = SW.Statisch.GetTitelX(TitelHerzog);
            SW.Dynamisch.GetAktHum().SetTaler(herzog.AbTaler);

            Assert.False(herzog.IstSpielerFuerTitelBerechtigt(SW.Dynamisch, SpielerId));
        }
    }
}
