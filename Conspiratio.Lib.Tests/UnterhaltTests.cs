using Conspiratio.Lib.Allgemein;
using Conspiratio.Lib.Gameplay.Niederlassung;
using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>
    /// Der Unterhalt ist die erste laufende Ausgabe, die am Besitz haengt statt an der Produktion.
    /// Bisher kostete eine stillgelegte Werkstatt nichts, weil Betriebskosten nur Staetten in
    /// Produktion zaehlen - Horten war gratis, Wirtschaften teuer.
    /// </summary>
    public class UnterhaltTests
    {
        /// <summary>Schaltet die ersten <paramref name="anzahl"/> Werkstattplaetze frei.</summary>
        private static void SetzeWerkstaetten(int anzahl)
        {
            var spieler = SW.Dynamisch.GetAktHum();
            int gesetzt = 0;

            for (int stadtId = 1; stadtId < SW.Statisch.GetMaxStadtID() && gesetzt < anzahl; stadtId++)
                for (int nr = 1; nr <= SW.Statisch.GetMaxWerkstaettenProStadt() && gesetzt < anzahl; nr++)
                {
                    spieler.GetSpielerHatInStadtXWerkstaettenY(nr, stadtId).SetEnabled(true);
                    gesetzt++;
                }
        }

        [Fact]
        public void Der_Unterhalt_steigt_progressiv_mit_der_Betriebszahl()
        {
            TestSpielwelt.Starte();
            SetzeWerkstaetten(20);

            // Nicht gegen die Literalzahl 20 pruefen: PlayerSetupManager schaltet beim Anlegen bereits
            // eine Werkstatt frei, und die liegt in der Heimatstadt - je nach deren ID ausserhalb der
            // ersten 20 durchlaufenen Plaetze. Der Test geht deshalb vom tatsaechlichen Bestand aus.
            int besessen = SW.Dynamisch.GetAktHum().ZaehleWerkstaetten();

            Assert.True(besessen >= 20, "Der Aufbau muss mindestens 20 Betriebe ergeben haben.");

            var ergebnis = new AbrechnungsManager().ErstelleAbrechnungFuerAktivenSpieler();

            // Der n-te Betrieb kostet Grundunterhalt * n, die Summe also N*(N+1)/2 Grundeinheiten.
            Assert.Equal(AbrechnungsManager.GrundunterhaltProWerkstatt * besessen * (besessen + 1) / 2,
                         ergebnis.Unterhalt);
        }

        [Fact]
        public void Der_Unterhalt_steckt_in_den_Gesamtkosten()
        {
            TestSpielwelt.Starte();
            SetzeWerkstaetten(5);

            var ergebnis = new AbrechnungsManager().ErstelleAbrechnungFuerAktivenSpieler();

            Assert.True(ergebnis.Unterhalt > 0, "Fuenf Betriebe muessen Unterhalt kosten.");
            Assert.True(ergebnis.Gesamtkosten >= ergebnis.Unterhalt, "Der Unterhalt muss in den Gesamtkosten stecken.");
        }

        /// <summary>
        /// Die Randbedingung des Vorhabens als Test: Ein Einsteiger mit zwei Betrieben darf den
        /// Unterhalt nicht spueren. Diese Schranke begrenzt die Kalibrierung von
        /// GrundunterhaltProWerkstatt nach unten.
        /// </summary>
        [Fact]
        public void Ein_Einsteiger_mit_zwei_Betrieben_zahlt_kaum_Unterhalt()
        {
            // Kein SetzeWerkstaetten: Ein frisch angelegter Spieler besitzt genau die eine Werkstatt,
            // die PlayerSetupManager ihm gibt. Genau das ist der Einsteigerfall.
            TestSpielwelt.Starte();

            var spieler = SW.Dynamisch.GetAktHum();
            spieler.GetSpielerHatInStadtXWerkstaettenY(1, 1).SetEnabled(true);

            int besessen = spieler.ZaehleWerkstaetten();

            Assert.Equal(2, besessen);

            var ergebnis = new AbrechnungsManager().ErstelleAbrechnungFuerAktivenSpieler();

            // Drei Grundeinheiten (1 + 2). Gemessen am Startkapital eines Spielers ist das Rauschen.
            Assert.Equal(AbrechnungsManager.GrundunterhaltProWerkstatt * 3, ergebnis.Unterhalt);
            Assert.True(ergebnis.Unterhalt <= 500, "Zwei Betriebe duerfen einen Einsteiger nicht spuerbar belasten.");
        }

        /// <summary>
        /// Die eigentliche Umkehrung: Betriebskosten zaehlen nur Staetten in Produktion, der Unterhalt
        /// zaehlt Besitz. Ein stillgelegter Betrieb kostet deshalb genauso viel wie ein laufender.
        /// </summary>
        [Fact]
        public void Der_Unterhalt_haengt_nicht_an_der_Produktion()
        {
            TestSpielwelt.Starte();
            SetzeWerkstaetten(4);

            int ohneProduktion = new AbrechnungsManager().ErstelleAbrechnungFuerAktivenSpieler().Unterhalt;

            TestSpielwelt.Starte();
            SetzeWerkstaetten(4);
            SW.Dynamisch.GetAktHum().GetProduktionsslot(1, 0)
              .SetTaetigkeit((int)EnumProduktionsslotAktionsart.Produzieren);

            int mitProduktion = new AbrechnungsManager().ErstelleAbrechnungFuerAktivenSpieler().Unterhalt;

            Assert.Equal(ohneProduktion, mitProduktion);
        }
    }
}
