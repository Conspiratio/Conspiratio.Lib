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
        private const int Buerger = 1;
        private const int Edelmann = 2;
        private const int Herzog = 9;

        /// <summary>
        /// Richtet <paramref name="gut"/> voll besetzte und <paramref name="schwach"/> halb besetzte
        /// Betriebe ein und liefert die Geltung zurueck, die die Jahresabrechnung dafuer gutschreibt.
        /// Der Spieler bleibt Buerger, damit der Hofhaltungsanteil 0 ist und allein die Auslastung
        /// gemessen wird.
        /// </summary>
        private static int GeltungAusAuslastung(int gut, int schwach)
        {
            TestSpielwelt.Starte();

            var spieler = SW.Dynamisch.GetAktHum();
            spieler.SetTitel(Buerger);
            spieler.SetTaler(1000000);

            int eingerichtet = 0;

            for (int stadtId = 1; stadtId < SW.Statisch.GetMaxStadtID() && eingerichtet < gut + schwach; stadtId++)
            {
                for (int slot = 0; slot < SW.Statisch.GetMaxProdSlots() && eingerichtet < gut + schwach; slot++)
                {
                    int rohstoffId = SW.Dynamisch.GetStadtwithID(stadtId).GetSingleRohstoff(slot + 1);
                    var ware = SW.Dynamisch.GetRohstoffwithID(rohstoffId);
                    int proStaette = ware.GetWerkstaetten() > 0 ? ware.GetArbeiter() / ware.GetWerkstaetten() : 0;

                    // Waren mit weniger als zwei Arbeitern je Staette taugen hier nicht: Bei 0 kennt die
                    // Abrechnung gar keinen Bedarf, bei 1 laesst sich keine halbe Besetzung stellen.
                    if (proStaette < 2)
                        continue;

                    var produktionsslot = spieler.GetProduktionsslot(stadtId, slot);

                    spieler.GetSpielerHatInStadtXWerkstaettenY(slot + 1, stadtId).SetEnabled(true);
                    produktionsslot.SetTaetigkeit((int)EnumProduktionsslotAktionsart.Produzieren);
                    produktionsslot.SetProduktionRohstoff(rohstoffId);
                    produktionsslot.SetProduktionStaetten(1);
                    produktionsslot.SetProduktionArbeiter(eingerichtet < gut ? proStaette : proStaette / 2);

                    eingerichtet++;
                }
            }

            Assert.Equal(gut + schwach, eingerichtet);

            int vorher = spieler.GetPermaAnsehen();
            new AbrechnungsManager().ErstelleAbrechnungFuerAktivenSpieler();

            return spieler.GetPermaAnsehen() - vorher;
        }

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
        /// sich mit einem einzigen aufwendigen Jahr jedes Ansehensniveau kaufen. Hofhaltung und
        /// Auslastung teilen sich dieselbe Obergrenze - deshalb traegt die Zuchtprobe hier zusaetzlich
        /// einen voll ausgelasteten Betrieb, sonst wuerde die Deckelung der Wechselwirkung beider
        /// Quellen nie geprueft.
        /// </summary>
        [Fact]
        public void Der_Ansehensgewinn_ist_auf_das_Jahresmaximum_gedeckelt()
        {
            TestSpielwelt.Starte();

            var spieler = SW.Dynamisch.GetAktHum();
            var slot = spieler.GetProduktionsslot(1, 0);
            int rohstoffId = SW.Dynamisch.GetStadtwithID(1).GetSingleRohstoff(1);
            var ware = SW.Dynamisch.GetRohstoffwithID(rohstoffId);
            int proStaette = ware.GetWerkstaetten() > 0 ? ware.GetArbeiter() / ware.GetWerkstaetten() : 1;

            spieler.SetTitel(Herzog);
            spieler.SetTaler(100000000);
            spieler.SetHofhaltungAbweichung(1);

            spieler.GetSpielerHatInStadtXWerkstaettenY(1, 1).SetEnabled(true);
            slot.SetTaetigkeit((int)EnumProduktionsslotAktionsart.Produzieren);
            slot.SetProduktionRohstoff(rohstoffId);
            slot.SetProduktionStaetten(1);
            slot.SetProduktionArbeiter(proStaette);

            int vorher = spieler.GetPermaAnsehen();
            new AbrechnungsManager().ErstelleAbrechnungFuerAktivenSpieler();

            Assert.True(spieler.GetPermaAnsehen() - vorher <= AbrechnungsManager.AnsehenMaxProJahr,
                        "Der Jahresgewinn darf das Maximum nicht ueberschreiten.");
            Assert.Equal(AbrechnungsManager.AnsehenMaxProJahr, spieler.GetPermaAnsehen() - vorher);
        }

        /// <summary>
        /// Auch ein Mehraufwand, der fuer keinen vollen Punkt reicht, muss Geltung bringen - sonst waere
        /// aufwendige Hofhaltung fuer die unteren Raenge wirkungslos. Edelmann: standesgemaess 500,
        /// aufwendig 1000, Differenz 500 - weit unter dem Kurs von 10 000 Talern je Punkt.
        /// </summary>
        [Fact]
        public void Kleiner_Mehraufwand_bringt_trotzdem_einen_Punkt()
        {
            TestSpielwelt.Starte();

            var spieler = SW.Dynamisch.GetAktHum();
            spieler.SetTitel(Edelmann);
            spieler.SetTaler(1000000);
            spieler.SetHofhaltungAbweichung(1);

            int vorher = spieler.GetPermaAnsehen();
            new AbrechnungsManager().ErstelleAbrechnungFuerAktivenSpieler();

            Assert.Equal(1, spieler.GetPermaAnsehen() - vorher);
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

        /// <summary>
        /// Die Belohnung muss mit der Betriebszahl wachsen. Zuvor war sie ein Mittelwert ueber die
        /// produzierenden Betriebe und damit massstabsfrei: Ein einziger gut besetzter Betrieb brachte
        /// genauso viel wie zwanzig - das Gegenteil der Absicht, die Geltung nach Auslastung *und*
        /// Betriebszahl zu staffeln.
        /// </summary>
        [Fact]
        public void Mehr_gut_gefuehrte_Betriebe_bringen_mehr_Geltung()
        {
            int einer = GeltungAusAuslastung(gut: 1, schwach: 0);
            int dreie = GeltungAusAuslastung(gut: 3, schwach: 0);

            Assert.True(einer > 0, "Ein gut gefuehrter Betrieb muss Geltung bringen.");
            Assert.True(dreie > einer,
                        "Drei gut gefuehrte Betriebe muessen mehr bringen als einer (gemessen: " +
                        dreie + " gegen " + einer + ").");
        }

        /// <summary>
        /// Die Gegenprobe zur Staffelung, und der eigentliche Fehler des Mittelwerts: Ein schwacher
        /// Betrieb zog den Schnitt unter die Schwelle, sein Stilllegen hob ihn wieder darueber. Der
        /// Anreiz war, die Messbasis zu verkleinern statt gut zu wirtschaften. Jetzt zaehlt jeder
        /// Betrieb fuer sich - der schwache bringt nichts, kostet aber auch nichts, und die guten
        /// zaehlen neben ihm unveraendert.
        /// </summary>
        [Fact]
        public void Stilllegen_eines_schwachen_Betriebs_hebt_die_Geltung_nicht()
        {
            int mitSchwachem = GeltungAusAuslastung(gut: 2, schwach: 1);
            int ohneSchwachen = GeltungAusAuslastung(gut: 2, schwach: 0);

            Assert.True(mitSchwachem > 0,
                        "Die gut gefuehrten Betriebe muessen auch neben einem schwachen zaehlen.");
            Assert.True(ohneSchwachen <= mitSchwachem,
                        "Stilllegen darf die Geltung nicht heben (gemessen: " + ohneSchwachen +
                        " nach dem Stilllegen gegen " + mitSchwachem + " davor).");
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

        /// <summary>
        /// Ansehen war faktisch der Kontostand: 2 500 Taler ergaben einen Punkt, bei 2,9 Mio. also
        /// 1 160 - gegen 3 bis 12 fuer ein Amt und 15 bis 60 fuer einen KI-Spieler insgesamt. In der
        /// Wahl (AemterManager, Ansehen/10) war das ein Vorsprung, den kein Gegner einholen konnte.
        /// AnsehenAktualisieren gibt es zudem nur auf HumSpieler - es war ein reiner Spielervorteil.
        /// </summary>
        [Fact]
        public void Der_Geldbestand_kauft_kein_Ansehen_mehr()
        {
            TestSpielwelt.Starte();

            var spieler = SW.Dynamisch.GetAktHum();

            spieler.SetTaler(10000);
            spieler.AnsehenAktualisieren();
            int arm = spieler.GetAnsehen();

            spieler.SetTaler(5000000);
            spieler.AnsehenAktualisieren();
            int reich = spieler.GetAnsehen();

            Assert.Equal(arm, reich);
        }
    }
}
