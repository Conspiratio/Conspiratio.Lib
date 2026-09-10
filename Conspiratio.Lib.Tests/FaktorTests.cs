using Conspiratio.Lib.Allgemein;
using Conspiratio.Lib.Gameplay.Niederlassung;
using Conspiratio.Lib.Gameplay.Personen;
using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>
    /// Der Faktor verkauft Übersicht und Vorausschau, nicht Ertrag – und kostet dafür jedes Jahr aufs
    /// Neue. Geprüft wird beides: dass der Lohn mit dem Betrieb wächst und in der Abrechnung landet,
    /// und dass die Prognose rechnet, was sie zu rechnen behauptet.
    /// </summary>
    public class FaktorTests
    {
        private const int Stadt = 1;
        private const int Slot = 0;
        private const int Korn = 1;

        private static FaktorManager Manager => new FaktorManager();

        private static HumSpieler Spieler => SW.Dynamisch.GetAktHum();

        /// <summary>Gibt dem Spieler in dieser Stadt eine Werkstätte auf dem angegebenen Platz.</summary>
        private static void GibWerkstatt(int stadtId, int nr = 1)
        {
            Spieler.GetSpielerHatInStadtXWerkstaettenY(nr, stadtId).SetEnabled(true);
        }

        /// <summary>
        /// Schaltet alle Werkstätten ab. Der Spieler startet mit einer, und zwar nicht in Stadt 1 –
        /// wer den Lohn gegen feste Zahlen prüfen will, muss den Bestand selbst herstellen.
        /// </summary>
        private static void LeereAlleWerkstaetten()
        {
            for (int stadtId = 1; stadtId < SW.Statisch.GetMaxStadtID(); stadtId++)
            {
                for (int nr = 1; nr <= SW.Statisch.GetMaxWerkstaettenProStadt(); nr++)
                    Spieler.GetSpielerHatInStadtXWerkstaettenY(nr, stadtId).SetEnabled(false);
            }
        }

        /// <summary>Richtet einen Exportauftrag ein: so viel Ware im Jahr von hier in die Zielstadt.</summary>
        private static void LiefereJaehrlich(int zielStadtId, int rohstoffId, int menge,
                                             int herkunftStadtId = Stadt, int slot = Slot)
        {
            var produktionsslot = Spieler.GetProduktionsslot(herkunftStadtId, slot);
            produktionsslot.SetTaetigkeit((int)EnumProduktionsslotAktionsart.Verkaufen);
            produktionsslot.SetVerkaufRohstoff(rohstoffId);
            produktionsslot.SetVerkaufStadt(zielStadtId);
            produktionsslot.SetVerkaufAnzahl(menge);
        }

        /// <summary>Stellt eine Stadt auf einen bestimmten Verbrauch und Vorrat ein.</summary>
        private static void RichteMarktEin(int stadtId, int rohstoffId, int einwohner, int vorrat)
        {
            var stadt = SW.Dynamisch.GetStadtwithID(stadtId);
            stadt.SetEinwohnerAufX(einwohner);
            stadt.SetRohstoffVorratWithIDXToY(rohstoffId, vorrat);
        }

        #region Lohn

        /// <summary>
        /// Der Lohn wächst mit den Märkten, die der Faktor im Blick behalten muss – nicht mit der Zahl
        /// der Werkstätten. Zwei Betriebe in derselben Stadt sind ein Markt.
        /// </summary>
        [Fact]
        public void Der_Lohn_waechst_mit_den_Staedten_nicht_mit_den_Werkstaetten()
        {
            TestSpielwelt.Starte();
            LeereAlleWerkstaetten();

            Assert.Equal(FaktorManager.Grundlohn, Manager.GetJahreslohn());

            // Zwei Werkstätten in derselben Stadt sind ein Markt – also ein Aufschlag, nicht zwei.
            GibWerkstatt(1);
            GibWerkstatt(1, nr: 2);

            Assert.Equal(FaktorManager.Grundlohn + FaktorManager.LohnJeStadt, Manager.GetJahreslohn());

            GibWerkstatt(2);
            GibWerkstatt(3);

            Assert.Equal(FaktorManager.Grundlohn + 3 * FaktorManager.LohnJeStadt,
                         Manager.GetJahreslohn());
        }

        [Fact]
        public void Der_Faktor_tritt_erst_auf_Anweisung_in_Dienst()
        {
            TestSpielwelt.Starte();

            Assert.False(Manager.IstAngestellt());

            Assert.True(Manager.StelleAn(out _));
            Assert.True(Manager.IstAngestellt());

            // Ein zweiter bringt nichts und wird abgewiesen.
            Assert.False(Manager.StelleAn(out _));

            Manager.Entlasse();
            Assert.False(Manager.IstAngestellt());
        }

        /// <summary>
        /// Der Lohn ist ein eigener Posten der Jahresabrechnung und wird erst dort fällig – die
        /// Anstellung selbst kostet nichts, denn bezahlt wird ein Jahr Arbeit und kein Handschlag.
        /// </summary>
        [Fact]
        public void Der_Lohn_steht_in_der_Jahresabrechnung()
        {
            TestSpielwelt.Starte();
            LeereAlleWerkstaetten();
            GibWerkstatt(1);

            var ohne = new AbrechnungsManager().ErstelleAbrechnungFuerAktivenSpieler();
            Assert.Equal(0, ohne.Faktorlohn);

            int talerVorher = Spieler.GetTaler();
            Manager.StelleAn(out _);

            Assert.Equal(talerVorher, Spieler.GetTaler());

            var mit = new AbrechnungsManager().ErstelleAbrechnungFuerAktivenSpieler();

            Assert.Equal(FaktorManager.Grundlohn + FaktorManager.LohnJeStadt, mit.Faktorlohn);
            Assert.Equal(ohne.Gesamtkosten + mit.Faktorlohn, mit.Gesamtkosten);
        }

        #endregion

        #region Übersicht

        /// <summary>
        /// Der Vergleich aller Städte ist das, was es sonst nirgends gibt: Die Zielstadt eines Exports
        /// wird einzeln durchgeschaltet. Sortiert nach Marktpreis, denn die Frage lautet „wohin damit".
        /// </summary>
        [Fact]
        public void Die_Uebersicht_zeigt_alle_Staedte_nach_Preis_sortiert()
        {
            TestSpielwelt.Starte();

            var zeilen = Manager.ErstelleMarktuebersicht(Korn);

            Assert.Equal(SW.Statisch.GetMaxStadtID() - 1, zeilen.Count);

            for (int i = 1; i < zeilen.Count; i++)
                Assert.True(zeilen[i - 1].Marktpreis >= zeilen[i].Marktpreis,
                            "Nicht absteigend sortiert bei " + i);
        }

        /// <summary>Die Zeile nennt Grundpreis und Marktpreis nebeneinander – der Abschlag geht auf.</summary>
        [Fact]
        public void Die_Zeile_rechnet_mit_sich_selbst_auf()
        {
            TestSpielwelt.Starte();
            RichteMarktEin(Stadt, Korn, einwohner: 1000, vorrat: 200);

            var zeile = Manager.ErstelleZeile(Stadt, Korn);

            Assert.Equal(100, zeile.Jahresbedarf);
            Assert.Equal(20, zeile.AbschlagProzent);   // 200 Vorrat = 2 Jahresbedarfe = 20 %
            Assert.Equal(zeile.Grundpreis * 80 / 100, zeile.Marktpreis);
            Assert.False(zeile.AmHoechstabschlag);
        }

        #endregion

        #region Prognose

        /// <summary>
        /// Die Rechnung des Konzepts: Der Überhang wächst um Lieferung minus Jahresbedarf, der Deckel
        /// liegt bei fünf Jahresbedarfen Vorrat. Hier also (500 − 100) / (300 − 100) = 2 Jahre.
        /// </summary>
        [Fact]
        public void Die_Prognose_rechnet_den_Ueberhang_hoch()
        {
            TestSpielwelt.Starte();
            RichteMarktEin(Stadt, Korn, einwohner: 1000, vorrat: 100);

            Assert.Equal(2, Manager.SchaetzeJahreBisHoechstabschlag(Stadt, Korn, jahreslieferung: 300));
        }

        /// <summary>Ein angebrochenes Jahr ist eines: Aufgerundet wird immer.</summary>
        [Fact]
        public void Die_Prognose_rundet_auf()
        {
            TestSpielwelt.Starte();
            RichteMarktEin(Stadt, Korn, einwohner: 1000, vorrat: 100);

            // (500 - 100) / (400 - 100) = 1,33 -> 2
            Assert.Equal(2, Manager.SchaetzeJahreBisHoechstabschlag(Stadt, Korn, jahreslieferung: 400));
        }

        /// <summary>
        /// Wer nicht mehr liefert, als die Stadt verbraucht, sättigt sie nie – der Vorrat wächst dann
        /// gar nicht. Das ist die Auskunft, die den Spieler seine Menge wählen lässt.
        /// </summary>
        [Fact]
        public void Wer_nur_den_Bedarf_deckt_saettigt_nie()
        {
            TestSpielwelt.Starte();
            RichteMarktEin(Stadt, Korn, einwohner: 1000, vorrat: 100);

            Assert.Equal(FaktorMarktzeile.NieErreicht,
                         Manager.SchaetzeJahreBisHoechstabschlag(Stadt, Korn, jahreslieferung: 100));
            Assert.Equal(FaktorMarktzeile.NieErreicht,
                         Manager.SchaetzeJahreBisHoechstabschlag(Stadt, Korn, jahreslieferung: 0));
        }

        [Fact]
        public void Am_Deckel_meldet_die_Prognose_null()
        {
            TestSpielwelt.Starte();
            RichteMarktEin(Stadt, Korn, einwohner: 1000, vorrat: 900);

            var zeile = Manager.ErstelleZeile(Stadt, Korn);

            Assert.True(zeile.AmHoechstabschlag);
            Assert.Equal(0, zeile.JahreBisHoechstabschlag);
            Assert.Equal(Conspiratio.Lib.Gameplay.Gebiete.Stadt.MaxAbschlagProzent, zeile.AbschlagProzent);
        }

        /// <summary>
        /// Gezählt wird, was der Spieler planmäßig in diese Stadt bringt – also seine Exportaufträge.
        /// Ein Auftrag auf eine andere Ware oder in eine andere Stadt zählt nicht mit.
        /// </summary>
        [Fact]
        public void Die_Jahreslieferung_zaehlt_nur_die_passenden_Auftraege()
        {
            TestSpielwelt.Starte();

            LiefereJaehrlich(zielStadtId: 2, rohstoffId: Korn, menge: 300, herkunftStadtId: 1, slot: 0);
            LiefereJaehrlich(zielStadtId: 3, rohstoffId: Korn, menge: 500, herkunftStadtId: 1, slot: 1);

            Assert.Equal(300, Manager.ErmittleEigeneJahreslieferung(2, Korn));
            Assert.Equal(500, Manager.ErmittleEigeneJahreslieferung(3, Korn));
            Assert.Equal(0, Manager.ErmittleEigeneJahreslieferung(2, Korn + 1));
            Assert.Equal(0, Manager.ErmittleEigeneJahreslieferung(4, Korn));
        }

        #endregion

        #region Warnungen

        /// <summary>Ohne Faktor keine Warnung – das ist der Unterschied, für den man ihn bezahlt.</summary>
        [Fact]
        public void Ohne_Faktor_keine_Warnung()
        {
            TestSpielwelt.Starte();
            RichteMarktEin(2, Korn, einwohner: 1000, vorrat: 900);
            LiefereJaehrlich(zielStadtId: 2, rohstoffId: Korn, menge: 300);

            Assert.Empty(Manager.ErmittleWarnungen());

            Manager.StelleAn(out _);

            Assert.Single(Manager.ErmittleWarnungen());
        }

        /// <summary>
        /// Gewarnt wird auch vorher: Ein Markt, der im nächsten Jahr den Deckel erreicht, ist der
        /// interessantere Fall – da lässt sich noch etwas ändern.
        /// </summary>
        [Fact]
        public void Der_Faktor_warnt_vor_dem_Deckel_nicht_erst_danach()
        {
            TestSpielwelt.Starte();
            Manager.StelleAn(out _);

            // Deckel bei 500, Vorrat 400, Zuwachs 200 im Jahr -> im naechsten Jahr erreicht.
            RichteMarktEin(2, Korn, einwohner: 1000, vorrat: 400);
            LiefereJaehrlich(zielStadtId: 2, rohstoffId: Korn, menge: 300);

            var warnungen = Manager.ErmittleWarnungen();

            Assert.Single(warnungen);
            Assert.Contains("bald gesättigt", warnungen[0]);

            // Weit vom Deckel entfernt schweigt er - sonst waere die Warnung Rauschen.
            RichteMarktEin(2, Korn, einwohner: 1000, vorrat: 0);

            Assert.Empty(Manager.ErmittleWarnungen());
        }

        #endregion
    }
}
