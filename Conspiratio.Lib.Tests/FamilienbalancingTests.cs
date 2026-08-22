using Conspiratio.Lib.Allgemein;
using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>
    /// Balancing des Familiensystems: die Sterblichkeit der Kinder und die Preisspanne der
    /// Werbegeschenke. Beides fiel beim Spielen unangenehm auf.
    /// </summary>
    public class FamilienbalancingTests
    {
        #region Kindestod

        /// <summary>
        /// Der frühe Kindestod ist ein Risiko pro Kind und Jahr. Er lag bei 10 % – ein Kind erreichte damit
        /// das 18. Lebensjahr nur mit rund 15 % Wahrscheinlichkeit. Der Test pinnt die Rate über eine große
        /// Stichprobe statt über die Konstante, damit auch eine Umstellung der Formel auffällt.
        /// </summary>
        [Fact]
        public void Der_Kindestod_trifft_rund_drei_Prozent_der_Kinder_pro_Jahr()
        {
            TestSpielwelt.Starte(seed: 4711);

            var familie = new FamilieManager();
            const int versuche = 20000;
            int tode = 0;

            for (int i = 0; i < versuche; i++)
            {
                // Genau ein Kind je Durchgang, damit die Anzahl der Meldungen die Todesrate ist.
                SW.Dynamisch.GetAktHum().SetKindX(SW.Statisch.GetMinKindSlotNr(), true, "Testkind", 1);

                tode += familie.PruefeKindestode().Count;
            }

            double rate = (double)tode / versuche;

            Assert.InRange(rate, 0.025, 0.035);
        }

        [Fact]
        public void Ein_Kind_ueberlebt_die_Kindheit_mehrheitlich()
        {
            TestSpielwelt.Starte(seed: 1234);

            var familie = new FamilieManager();
            const int kinder = 2000;
            int ueberlebende = 0;

            for (int i = 0; i < kinder; i++)
            {
                SW.Dynamisch.GetAktHum().SetKindX(SW.Statisch.GetMinKindSlotNr(), true, "Testkind", 0);

                bool lebt = true;

                for (int jahr = 0; jahr < 18 && lebt; jahr++)
                    lebt = familie.PruefeKindestode().Count == 0;

                if (lebt)
                    ueberlebende++;
            }

            // Bei 3 % pro Jahr sind das rechnerisch 0,97^18 = 58 %.
            Assert.InRange((double)ueberlebende / kinder, 0.5, 0.66);
        }

        #endregion

        #region Werbegeschenke

        /// <summary>
        /// Die teure Geschenkstufe reichte von 1 000 bis 10 000 Taler Basispreis und wurde gleichverteilt
        /// gezogen – einem armen Spieler wurde dadurch ein königliches Diadem für über 10 000 Taler
        /// angeboten. Das Angebot richtet sich jetzt nach dem Vermögen.
        /// </summary>
        [Fact]
        public void Ein_armer_Spieler_bekommt_kein_uebermaessig_teures_Geschenk()
        {
            TestSpielwelt.Starte(seed: 42);
            RichteWerbungEin(vermoegen: 20000);

            var familie = new FamilieManager();

            for (int i = 0; i < 200; i++)
            {
                var auswahl = familie.ErstelleGeschenkAuswahl();
                int teuerstes = auswahl.Geschenke[2].Preis;

                // Höchstens ein Zehntel des Gesamtvermögens (plus der vermögensabhängige Zuschlag).
                Assert.True(teuerstes <= 4000, $"Teuerstes Geschenk war {teuerstes} Taler bei 20 000 Talern Vermögen");
            }
        }

        [Fact]
        public void Ein_reicher_Spieler_bekommt_auch_die_teuersten_Geschenke_angeboten()
        {
            TestSpielwelt.Starte(seed: 42);
            RichteWerbungEin(vermoegen: 2000000);

            var familie = new FamilieManager();
            int hoechsterPreis = 0;

            for (int i = 0; i < 200; i++)
                hoechsterPreis = System.Math.Max(hoechsterPreis, familie.ErstelleGeschenkAuswahl().Geschenke[2].Preis);

            // Das teuerste Geschenk der Tabelle hat 10 000 Taler Basispreis.
            Assert.True(hoechsterPreis >= 10000, $"Teuerstes angebotenes Geschenk war nur {hoechsterPreis} Taler");
        }

        [Fact]
        public void Es_werden_immer_drei_Geschenke_angeboten()
        {
            TestSpielwelt.Starte(seed: 42);
            RichteWerbungEin(vermoegen: 0);

            var auswahl = new FamilieManager().ErstelleGeschenkAuswahl();

            Assert.Equal(3, auswahl.Geschenke.Count);
        }

        /// <summary>Macht den aktiven Spieler zum Werbenden um eine KI und setzt sein Barvermögen.</summary>
        private static void RichteWerbungEin(int vermoegen)
        {
            var spieler = SW.Dynamisch.GetAktHum();

            spieler.WirbtUmSpielerID = SW.Statisch.GetMinKIID();
            spieler.ErhoeheTaler(vermoegen - spieler.GetTaler());
        }

        #endregion
    }
}
