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
        /// Der Verkaufspreis hängt bewusst <b>nicht</b> an der Staffelung, sondern an drei Vierteln des
        /// ungestaffelten Grundpreises der Ware. Der Staffelfaktor sitzt auf dem Warengrundpreis, der
        /// Zähler läuft aber reichsweit über alle Betriebe – ein gestaffelter Erlös hinge daher davon ab,
        /// wie viele <i>andere</i> Betriebe man besitzt, statt davon, was dieser gekostet hat.
        /// </summary>
        [Fact]
        public void Der_Verkaufspreis_ignoriert_die_Staffelung()
        {
            TestSpielwelt.Starte();

            var handel = new HandelsManager();

            SetzeWerkstaetten(0);
            Assert.Equal(1500, handel.GetWerkstattVerkaufspreis(GrosseStadt, 1));   // 2000 * 3 / 4

            SetzeWerkstaetten(4);
            Assert.Equal(4882, handel.GetWerkstattKaufpreis(GrosseStadt, 1));       // der Kaufpreis staffelt
            Assert.Equal(1500, handel.GetWerkstattVerkaufspreis(GrosseStadt, 1));   // der Erlös nicht
        }

        /// <summary>
        /// Sucht einen Werkstattplatz, an dem eine Ware mit genau diesem Grundpreis produziert wird
        /// (2 000 = Stufe 1, 10 000 = Stufe 2, 40 000 = Stufe 3).
        /// </summary>
        private static bool FindePlatzMitGrundpreis(int grundpreis, out int stadtId, out int werkstattNr)
        {
            var handel = new HandelsManager();

            for (stadtId = 1; stadtId < SW.Statisch.GetMaxStadtID(); stadtId++)
            {
                for (werkstattNr = 1; werkstattNr <= SW.Statisch.GetMaxWerkstaettenProStadt(); werkstattNr++)
                {
                    int rohstoffId = handel.RohstoffIdAnPlatz(stadtId, werkstattNr);

                    if (rohstoffId > 0 && SW.Dynamisch.GetRohstoffwithID(rohstoffId).GetWSKaufpreis() == grundpreis)
                        return true;
                }
            }

            stadtId = 0;
            werkstattNr = 0;
            return false;
        }

        /// <summary>
        /// Die Probe auf die Geldpumpe, die alle bisherigen – durchweg <b>einstufigen</b> – Tests übersehen
        /// haben: Bei gleicher Warenstufe verliert ein Rückkauf-Zyklus immer 25 %, der Gewinn entstand
        /// ausschließlich beim <b>Mischen der Stufen</b>. Hing der Erlös an der Staffelung, ließ sich so
        /// beliebig viel Geld erzeugen: billige Stufe-1-Betriebe verkaufen (der reichsweite Zähler
        /// sinkt), den teuren Stufe-3-Betrieb günstig kaufen, die billigen zurückkaufen (der Zähler
        /// steigt) und den teuren zum überhöhten Preis verkaufen.
        /// </summary>
        [Fact]
        public void Kein_Zyklus_ueber_verschiedene_Rohstoffstufen_wirft_Gewinn_ab()
        {
            TestSpielwelt.Starte();

            Assert.True(FindePlatzMitGrundpreis(2000, out int billigStadt, out int billigNr),
                        "Es muss einen Werkstattplatz mit einer Stufe-1-Ware geben.");
            Assert.True(FindePlatzMitGrundpreis(40000, out int teuerStadt, out int teuerNr),
                        "Es muss einen Werkstattplatz mit einer Stufe-3-Ware geben.");

            var handel = new HandelsManager();
            const int Billige = 12;
            int taler = 0;

            // 1. Alle billigen Betriebe verkaufen. VerkaufeWerkstatt liest den Preis, bevor der Platz
            //    deaktiviert wird – der verkaufte Betrieb zählt also noch mit.
            for (int besessen = Billige; besessen > 0; besessen--)
            {
                SetzeWerkstaetten(besessen);
                taler += handel.GetWerkstattVerkaufspreis(billigStadt, billigNr);
            }

            // 2. Ohne Besitzstand den teuren Stufe-3-Betrieb kaufen.
            SetzeWerkstaetten(0);
            taler -= handel.GetWerkstattKaufpreis(teuerStadt, teuerNr);

            // 3. Die billigen zurückkaufen; der teure Betrieb zählt dabei schon mit.
            for (int besessen = 1; besessen <= Billige; besessen++)
            {
                SetzeWerkstaetten(besessen);
                taler -= handel.GetWerkstattKaufpreis(billigStadt, billigNr);
            }

            // 4. Den teuren Betrieb beim jetzt hohen Zählerstand wieder verkaufen.
            SetzeWerkstaetten(Billige + 1);
            taler += handel.GetWerkstattVerkaufspreis(teuerStadt, teuerNr);

            Assert.True(taler < 0,
                        "Ein Kauf-/Verkaufszyklus über verschiedene Rohstoffstufen muss verlieren, sonst " +
                        "ist er eine beliebig wiederholbare Geldpumpe (Ergebnis: " + taler + ").");
        }

        /// <summary>
        /// Der allgemeine Grund, warum es eine solche Pumpe nicht geben kann: Der Erlös liegt nie über
        /// dem günstigsten je zahlbaren Kaufpreis derselben Ware, dem ungestaffelten Grundpreis.
        /// </summary>
        [Fact]
        public void Der_Erloes_liegt_nie_ueber_dem_guenstigsten_Kaufpreis()
        {
            TestSpielwelt.Starte();

            var handel = new HandelsManager();

            for (int stadtId = 1; stadtId < SW.Statisch.GetMaxStadtID(); stadtId++)
            {
                for (int nr = 1; nr <= SW.Statisch.GetMaxWerkstaettenProStadt(); nr++)
                {
                    if (handel.RohstoffIdAnPlatz(stadtId, nr) <= 0)
                        continue;

                    SetzeWerkstaetten(0);
                    int guenstigsterKaufpreis = handel.GetWerkstattKaufpreis(stadtId, nr);

                    for (int besessen = 0; besessen <= HandelsManager.MaxSteigerungsstufen + 5; besessen++)
                    {
                        SetzeWerkstaetten(besessen);

                        Assert.True(handel.GetWerkstattVerkaufspreis(stadtId, nr) <= guenstigsterKaufpreis,
                                    "Stadt " + stadtId + ", Platz " + nr + ", " + besessen + " Betriebe.");
                    }
                }
            }
        }

        /// <summary>
        /// Der Divisionsschutz <c>Math.Max(1, Einwohner / 10)</c> in <c>GetRohstoffPreisVonIDX</c>: Eine
        /// von Katastrophen entvölkerte Stadt hat einen Jahresbedarf von 0 und würde den Getter sonst
        /// durch null teilen lassen. Bislang deckte ihn kein Test ab.
        /// </summary>
        [Fact]
        public void Eine_entvoelkerte_Stadt_teilt_nicht_durch_null()
        {
            TestSpielwelt.Starte();

            var stadt = BereiteStadtVor(GrosseStadt, 0);
            stadt.SetEinwohnerAufX(0);

            // Ohne Vorrat bleibt der Basispreis stehen – entscheidend ist, dass kein Fehler fliegt.
            Assert.Equal(Basispreis, stadt.GetRohstoffPreisVonIDX(Korn));

            // Der Nenner klemmt auf 1: Schon ein einziges Stück Vorrat gilt als ein voller Jahresbedarf
            // und kostet damit einen ganzen Abschlagsschritt.
            stadt.SetRohstoffVorratWithIDXToY(Korn, 1);
            Assert.Equal((Basispreis * (100 - Stadt.AbschlagJeBedarfsjahrProzent)) / 100,
                         stadt.GetRohstoffPreisVonIDX(Korn));

            // Ab fünf Stück ist der Deckel erreicht - ohne den Schutz wäre hier durch null geteilt worden.
            stadt.SetRohstoffVorratWithIDXToY(Korn, 5);
            Assert.Equal((Basispreis * (100 - Stadt.MaxAbschlagProzent)) / 100, stadt.GetRohstoffPreisVonIDX(Korn));
        }
    }
}
