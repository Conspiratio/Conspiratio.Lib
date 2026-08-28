using Conspiratio.Lib.Allgemein;
using Conspiratio.Lib.Gameplay.Niederlassung;
using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>
    /// Geltung laesst sich erwirtschaften oder erkaufen: Wer aufwendig Hof haelt, gewinnt permanentes
    /// Ansehen; wer spart, verliert es. Die Waehrung ist PermaAnsehen - dieselbe, in der Kirchenaustritt,
    /// Kerker und Pranger rechnen.
    /// </summary>
    public class AnsehenTests
    {
        private const int Herzog = 9;

        [Fact]
        public void Aufwendige_Hofhaltung_bringt_permanentes_Ansehen()
        {
            TestSpielwelt.Starte();

            var spieler = SW.Dynamisch.GetAktHum();
            spieler.SetTitel(Herzog);
            spieler.SetTaler(1000000);
            spieler.SetHofhaltungAbweichung(1);

            int vorher = spieler.GetPermaAnsehen();
            new AbrechnungsManager().ErstelleAbrechnungFuerAktivenSpieler();

            Assert.True(spieler.GetPermaAnsehen() > vorher, "Mehraufwand muss Geltung bringen.");
        }

        [Fact]
        public void Sparsame_Hofhaltung_kostet_permanentes_Ansehen()
        {
            TestSpielwelt.Starte();

            var spieler = SW.Dynamisch.GetAktHum();
            spieler.SetTitel(Herzog);
            spieler.SetTaler(1000000);
            spieler.SetHofhaltungAbweichung(-1);

            int vorher = spieler.GetPermaAnsehen();
            new AbrechnungsManager().ErstelleAbrechnungFuerAktivenSpieler();

            Assert.True(spieler.GetPermaAnsehen() < vorher, "Wer spart, verliert Geltung.");
        }

        /// <summary>
        /// Der Gewinn ist gedeckelt: Geltung waechst ueber Jahre, nicht in einem Zug. Ohne Deckel liesse
        /// sich mit einem einzigen aufwendigen Jahr jedes Ansehensniveau kaufen.
        /// </summary>
        [Fact]
        public void Der_Ansehensgewinn_ist_auf_das_Jahresmaximum_gedeckelt()
        {
            TestSpielwelt.Starte();

            var spieler = SW.Dynamisch.GetAktHum();
            spieler.SetTitel(Herzog);
            spieler.SetTaler(100000000);
            spieler.SetHofhaltungAbweichung(1);

            int vorher = spieler.GetPermaAnsehen();
            new AbrechnungsManager().ErstelleAbrechnungFuerAktivenSpieler();

            Assert.True(spieler.GetPermaAnsehen() - vorher <= AbrechnungsManager.AnsehenMaxProJahr,
                        "Der Jahresgewinn darf das Maximum nicht ueberschreiten.");
        }

        /// <summary>
        /// Die Gegenrichtung zum Unterhalt: Der bestraft nur, wer schlecht auslastet. Damit gutes
        /// Wirtschaften auch gewinnt, bringt hohe Auslastung Geltung - erwirtschaftet statt erkauft.
        /// </summary>
        [Fact]
        public void Gut_ausgelastete_Werke_bringen_Geltung()
        {
            TestSpielwelt.Starte();

            var spieler = SW.Dynamisch.GetAktHum();
            var slot = spieler.GetProduktionsslot(1, 0);
            int rohstoffId = SW.Dynamisch.GetStadtwithID(1).GetSingleRohstoff(1);
            var ware = SW.Dynamisch.GetRohstoffwithID(rohstoffId);
            int proStaette = ware.GetWerkstaetten() > 0 ? ware.GetArbeiter() / ware.GetWerkstaetten() : 1;

            spieler.GetSpielerHatInStadtXWerkstaettenY(1, 1).SetEnabled(true);
            slot.SetTaetigkeit((int)EnumProduktionsslotAktionsart.Produzieren);
            slot.SetProduktionRohstoff(rohstoffId);
            slot.SetProduktionStaetten(1);
            slot.SetProduktionArbeiter(proStaette);

            int vorher = spieler.GetPermaAnsehen();
            new AbrechnungsManager().ErstelleAbrechnungFuerAktivenSpieler();

            Assert.True(spieler.GetPermaAnsehen() > vorher, "Volle Auslastung muss Geltung bringen.");
        }

        /// <summary>Ein stillgelegter Betrieb bringt nichts - die Belohnung wirkt nur nach oben.</summary>
        [Fact]
        public void Stillgelegte_Werke_bringen_keine_Geltung()
        {
            TestSpielwelt.Starte();

            var spieler = SW.Dynamisch.GetAktHum();
            spieler.GetSpielerHatInStadtXWerkstaettenY(1, 1).SetEnabled(true);

            int vorher = spieler.GetPermaAnsehen();
            new AbrechnungsManager().ErstelleAbrechnungFuerAktivenSpieler();

            Assert.Equal(vorher, spieler.GetPermaAnsehen());
        }
    }
}
