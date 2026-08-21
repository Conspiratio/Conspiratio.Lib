using Conspiratio.Lib.Allgemein;
using Conspiratio.Lib.Gameplay.Gebiete;
using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>
    /// Balancing des Warenhandels: Der Preis soll auf Übersättigung eines Marktes reagieren, damit
    /// mehrere Kontore nicht beliebig skalieren. Gemessen wird der Abschlag in Jahren lokalen Bedarfs
    /// (<c>Einwohner / 10</c>), nicht in festen Talern – so skaliert er mit der Stadtgröße.
    /// </summary>
    public class HandelsbalancingTests
    {
        private const int Korn = 1;

        /// <summary>Stadt 1 („Frozen Castle") hat 5 000 Einwohner, also 500 Stück Jahresbedarf.</summary>
        private const int GrosseStadt = 1;

        /// <summary>Stadt 2 („Icepike") hat 2 500 Einwohner, also 250 Stück Jahresbedarf.</summary>
        private const int KleineStadt = 2;

        /// <summary>Basispreis, den die Tests setzen – innerhalb von Korns Korridor [7, 20].</summary>
        private const int Basispreis = 8;

        private static Stadt BereiteStadtVor(int stadtId, int vorrat)
        {
            var stadt = SW.Dynamisch.GetStadtwithID(stadtId);
            stadt.SetRohstoffPreisVonIDXToY(Korn, Basispreis);
            stadt.SetRohstoffVorratWithIDXToY(Korn, vorrat);
            return stadt;
        }

        [Fact]
        public void Ohne_Vorrat_bleibt_der_Basispreis_unveraendert()
        {
            TestSpielwelt.Starte();

            Assert.Equal(Basispreis, BereiteStadtVor(GrosseStadt, 0).GetRohstoffPreisVonIDX(Korn));
        }

        [Fact]
        public void Ein_Jahresbedarf_Vorrat_kostet_genau_einen_Abschlagsschritt()
        {
            TestSpielwelt.Starte();

            // 500 Stück = ein Jahresbedarf von Stadt 1 => 10 % Abschlag => 8 * 90 / 100 = 7.
            Assert.Equal(7, BereiteStadtVor(GrosseStadt, 500).GetRohstoffPreisVonIDX(Korn));
        }

        [Fact]
        public void Der_Abschlag_ist_gedeckelt()
        {
            TestSpielwelt.Starte();

            // Selbst bei absurdem Vorrat greift nur MaxAbschlagProzent => 8 * 50 / 100 = 4.
            Assert.Equal(4, BereiteStadtVor(GrosseStadt, 1_000_000).GetRohstoffPreisVonIDX(Korn));
        }

        /// <summary>
        /// Die bewusste Verhaltensänderung: Früher klemmte der Getter bei <c>preisMin</c> und machte den
        /// Mengenabschlag damit wirkungslos. Jetzt begrenzt nur noch <c>MaxAbschlagProzent</c>.
        /// </summary>
        [Fact]
        public void Bei_Uebersaettigung_faellt_der_Preis_unter_den_Mindestpreis()
        {
            TestSpielwelt.Starte();

            int preis = BereiteStadtVor(GrosseStadt, 1_000_000).GetRohstoffPreisVonIDX(Korn);

            Assert.True(preis < SW.Dynamisch.GetRohstoffwithID(Korn).GetPreisMin(),
                        "Der Mengenabschlag muss unter preisMin wirken dürfen, sonst bleibt er folgenlos.");
        }

        [Fact]
        public void Eine_groessere_Stadt_verkraftet_denselben_Vorrat_besser()
        {
            TestSpielwelt.Starte();

            int gross = BereiteStadtVor(GrosseStadt, 500).GetRohstoffPreisVonIDX(Korn);
            int klein = BereiteStadtVor(KleineStadt, 500).GetRohstoffPreisVonIDX(Korn);

            // Gleicher Vorrat, halber Bedarf => doppelter Abschlag: 10 % gegen 20 %.
            Assert.Equal(7, gross);
            Assert.Equal(6, klein);
        }

        /// <summary>
        /// Setzt genau <paramref name="anzahl"/> Werkstätten des aktiven Spielers auf aktiv und alle
        /// übrigen auf inaktiv. Gültige Stadt-IDs sind 1 bis <c>GetMaxStadtID() - 1</c>.
        /// </summary>
        private static void SetzeWerkstaetten(int anzahl)
        {
            var spieler = SW.Dynamisch.GetAktHum();
            int gesetzt = 0;

            for (int stadtId = 1; stadtId < SW.Statisch.GetMaxStadtID(); stadtId++)
            {
                for (int nr = 1; nr <= SW.Statisch.GetMaxWerkstaettenProStadt(); nr++)
                {
                    bool aktiv = gesetzt < anzahl;
                    spieler.GetSpielerHatInStadtXWerkstaettenY(nr, stadtId).SetEnabled(aktiv);

                    if (aktiv)
                        gesetzt++;
                }
            }
        }

        [Fact]
        public void Der_erste_Betrieb_kostet_unveraendert_den_Grundpreis()
        {
            TestSpielwelt.Starte();
            SetzeWerkstaetten(0);

            // Stadt 1, Platz 1 produziert Holz (Stufe 1) => Grundpreis 2000.
            Assert.Equal(2000, new HandelsManager().GetWerkstattKaufpreis(GrosseStadt, 1));
        }

        /// <summary>
        /// Die Staffelung rechnet in Ganzzahlen und schneidet dabei je Schritt ab; das Ergebnis liegt
        /// daher etwas unter <c>2000 * 1,25^n</c>. Geprüft wird gegen die tatsächliche Schrittfolge.
        /// </summary>
        [Theory]
        [InlineData(0, 2000)]
        [InlineData(4, 4882)]
        [InlineData(9, 14895)]
        [InlineData(14, 45452)]
        public void Jeder_weitere_Betrieb_wird_teurer(int besessen, int erwarteterPreis)
        {
            TestSpielwelt.Starte();
            SetzeWerkstaetten(besessen);

            Assert.Equal(erwarteterPreis, new HandelsManager().GetWerkstattKaufpreis(GrosseStadt, 1));
        }

        /// <summary>
        /// Ohne Deckel liefe <c>int</c> ab etwa dem 62. Betrieb über – besitzbar sind 84.
        /// </summary>
        [Fact]
        public void Jenseits_des_Deckels_bleibt_der_Preis_endlich()
        {
            TestSpielwelt.Starte();
            SetzeWerkstaetten(84);

            int preis = new HandelsManager().GetWerkstattKaufpreis(GrosseStadt, 1);

            Assert.Equal(173382, preis);
        }

        [Fact]
        public void Der_Zaehler_erfasst_Betriebe_ueber_Stadtgrenzen_hinweg()
        {
            TestSpielwelt.Starte();
            SetzeWerkstaetten(0);

            var spieler = SW.Dynamisch.GetAktHum();
            spieler.GetSpielerHatInStadtXWerkstaettenY(1, GrosseStadt).SetEnabled(true);
            spieler.GetSpielerHatInStadtXWerkstaettenY(3, KleineStadt).SetEnabled(true);

            Assert.Equal(2, spieler.ZaehleWerkstaetten());
        }

        [Fact]
        public void Deaktivierte_Plaetze_zaehlen_nicht_mit()
        {
            TestSpielwelt.Starte();
            SetzeWerkstaetten(5);

            SW.Dynamisch.GetAktHum().GetSpielerHatInStadtXWerkstaettenY(1, GrosseStadt).SetEnabled(false);

            Assert.Equal(4, SW.Dynamisch.GetAktHum().ZaehleWerkstaetten());
        }

        /// <summary>
        /// Der Verkaufspreis bleibt an den Kaufpreis gekoppelt (¾) und erbt die Staffelung damit
        /// automatisch. Beabsichtigt: Wer verkauft, bekommt anteilig zurück, was er bezahlt hat. Eine
        /// Rückkauf-Arbitrage entsteht nicht, weil ¾ kleiner als 1 ist.
        /// </summary>
        [Fact]
        public void Der_Verkaufspreis_folgt_der_Staffelung()
        {
            TestSpielwelt.Starte();
            SetzeWerkstaetten(4);

            var handel = new HandelsManager();

            Assert.Equal(4882, handel.GetWerkstattKaufpreis(GrosseStadt, 1));
            Assert.Equal(4882 * 3 / 4, handel.GetWerkstattVerkaufspreis(GrosseStadt, 1));
        }
    }
}
