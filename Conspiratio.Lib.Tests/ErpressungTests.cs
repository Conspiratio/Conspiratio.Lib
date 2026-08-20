using Conspiratio.Lib.Allgemein;
using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>
    /// Erpressung von Amtsträgern (WinForms-Issue #13). Die Zahlen stammen aus dem Konzept
    /// „Konzept_Erpressen.pdf"; die drei Szenarien seiner Tabelle sind als exakte Zusicherungen
    /// festgehalten, damit ein Balancing-Eingriff nicht unbemerkt davon abweicht.
    /// </summary>
    public class ErpressungTests
    {
        private const int AmtStadt = 7;    // Bürgermeister
        private const int AmtLand = 30;
        private const int AmtReich = 44;

        /// <summary>
        /// Konzept-Tabelle: 7 Beweispunkte, Ziel zwei Titelstufen über dem Erpresser, gleiche Religion.
        /// Erwartet 56 % (Stadt), 48 % (Grafschaft), 40 % (Königreich).
        /// </summary>
        [Theory]
        [InlineData(AmtStadt, 3, 56)]
        [InlineData(AmtLand, 5, 48)]
        [InlineData(AmtReich, 7, 40)]
        public void Erfolgschance_entspricht_dem_Konzept(int amtId, int erwarteteMindestpunkte, int erwarteteChance)
        {
            TestSpielwelt.Starte();
            int zielId = TestSpielwelt.SetzeKiGegner(0, amtId);

            var spieler = SW.Dynamisch.GetAktHum();
            var ziel = SW.Dynamisch.GetSpWithID(zielId);

            spieler.SetTitel(1);
            spieler.SetReligion(SW.Statisch.GetRelKathID());
            ziel.SetTitel(3);
            ziel.SetReligion(SW.Statisch.GetRelKathID());

            TestSpielwelt.GibBeweise(zielId, 7);

            var manager = new ErpressungManager();

            Assert.Equal(erwarteteMindestpunkte, manager.GetMindestpunkte(zielId));
            Assert.Equal(erwarteteChance, manager.BerechneErfolgschance(zielId));
        }

        [Fact]
        public void Zu_wenige_Beweise_verhindern_die_Erpressung()
        {
            TestSpielwelt.Starte();
            int zielId = TestSpielwelt.SetzeKiGegner(0, AmtStadt);

            var manager = new ErpressungManager();

            TestSpielwelt.GibBeweise(zielId, ErpressungManager.MindestpunkteStadt - 1);
            Assert.False(manager.KannErpressen(zielId, out string grund));
            Assert.Contains("Beweise", grund);

            TestSpielwelt.GibBeweise(zielId, ErpressungManager.MindestpunkteStadt);
            Assert.True(manager.KannErpressen(zielId, out _));
        }

        [Fact]
        public void Ohne_Spionage_ist_keine_Erpressung_moeglich()
        {
            TestSpielwelt.Starte();
            int zielId = TestSpielwelt.SetzeKiGegner(0, AmtStadt);

            TestSpielwelt.GibBeweise(zielId, 9);
            SW.Dynamisch.GetAktHum().GetAktiveSpionage(zielId).SetKosten(0);

            Assert.False(new ErpressungManager().KannErpressen(zielId, out _));
        }

        [Fact]
        public void Wer_kein_Amt_hat_ist_nicht_erpressbar()
        {
            TestSpielwelt.Starte();
            int zielId = TestSpielwelt.SetzeKiGegner(0, 0);

            TestSpielwelt.GibBeweise(zielId, 9);

            Assert.False(new ErpressungManager().KannErpressen(zielId, out _));
        }

        /// <summary>Grunddauer 4/3/2 Jahre plus Zufall 0–3 / 0–2 / 0–1 (im Issue gegenüber dem Konzept gekürzt).</summary>
        [Theory]
        [InlineData(AmtStadt, 4, 7)]
        [InlineData(AmtLand, 3, 5)]
        [InlineData(AmtReich, 2, 3)]
        public void Wirkungsdauer_bleibt_in_der_erwarteten_Spanne(int amtId, int min, int max)
        {
            TestSpielwelt.Starte();
            int zielId = TestSpielwelt.SetzeKiGegner(0, amtId);

            var manager = new ErpressungManager();
            int kleinste = int.MaxValue, groesste = int.MinValue;

            for (int lauf = 0; lauf < 1000; lauf++)
            {
                int dauer = manager.BerechneDauer(zielId);
                kleinste = System.Math.Min(kleinste, dauer);
                groesste = System.Math.Max(groesste, dauer);
            }

            Assert.Equal(min, kleinste);
            Assert.Equal(max, groesste);
        }

        [Fact]
        public void Erfolgreiche_Erpressung_verbraucht_die_Beweise_und_bucht_ein_Delikt()
        {
            TestSpielwelt.Starte();
            int zielId = TestSpielwelt.SetzeKiGegner(0, AmtStadt);
            TestSpielwelt.GibBeweise(zielId, 9);

            var spieler = SW.Dynamisch.GetAktHum();
            int gesetz = SW.Statisch.GetGesetzErpressung();
            int deliktVorher = spieler.GetBegingVerbrechenX(gesetz);

            var ergebnis = new ErpressungManager().FuehreErpressungDurch(zielId, true);

            Assert.True(ergebnis.Erfolg);
            Assert.InRange(ergebnis.Jahre, 4, 7);
            Assert.True(spieler.ErpresstBereits(zielId));
            Assert.True(SW.Dynamisch.WirdErpresst(zielId));
            Assert.Equal(0, spieler.GetAktiveSpionage(zielId).GetDelikte());
            Assert.Equal(deliktVorher + 1, spieler.GetBegingVerbrechenX(gesetz));
        }

        [Fact]
        public void Dasselbe_Opfer_wird_nicht_zweimal_erpresst()
        {
            TestSpielwelt.Starte();
            int zielId = TestSpielwelt.SetzeKiGegner(0, AmtStadt);
            TestSpielwelt.GibBeweise(zielId, 9);

            var manager = new ErpressungManager();
            manager.FuehreErpressungDurch(zielId, true);

            TestSpielwelt.GibBeweise(zielId, 9);
            Assert.False(manager.KannErpressen(zielId, out _));
        }

        [Fact]
        public void Gescheiterte_Erpressung_kostet_bei_der_KI_Beweise_und_Ansehen()
        {
            TestSpielwelt.Starte();
            int zielId = TestSpielwelt.SetzeKiGegner(0, AmtStadt);
            TestSpielwelt.GibBeweise(zielId, 9);

            var ki = SW.Dynamisch.GetKIwithID(zielId);

            // Die Startbeziehung wird je Spiel ausgewürfelt (CreateRndBeziehungen zieht 20–80, danach
            // verschiebt der Weltaufbau nochmals um −20…+20) und kann damit unter dem Verlustbetrag
            // liegen. ErhoeheBeziehungZuX kappt bei 0, sodass die erwartete Differenz dann unerreichbar
            // ist – genau daran scheiterte dieser Test in rund 8 % der Läufe. Deshalb einen festen, von
            // beiden Grenzen (0/100) entfernten Startwert setzen, damit die exakte Verlusthöhe prüfbar
            // bleibt, ohne vom Zufall abzuhängen.
            const int beziehungVorher = 90;
            ki.SetBeziehungZuX(SW.Dynamisch.GetAktiverSpieler(), beziehungVorher);

            var ergebnis = new ErpressungManager().FuehreErpressungDurch(zielId, false);

            Assert.False(ergebnis.Erfolg);
            Assert.False(ergebnis.BeweiseBleiben);
            Assert.Equal(0, SW.Dynamisch.GetAktHum().GetAktiveSpionage(zielId).GetDelikte());
            Assert.Equal(beziehungVorher - ErpressungManager.BeziehungsverlustBeiMisserfolg,
                         ki.GetBeziehungZuKIX(SW.Dynamisch.GetAktiverSpieler()));
        }

        /// <summary>
        /// PvP-Sonderregel aus dem Issue: Lehnt ein menschliches Opfer ab, bleiben dem Erpresser die
        /// Beweise – sonst gäbe es für den Erpressten keinen Grund, sich je zu beugen.
        /// </summary>
        [Fact]
        public void Lehnt_ein_Mensch_ab_bleiben_die_Beweise_erhalten()
        {
            TestSpielwelt.Starte(menschen: 2);
            SW.Dynamisch.GetHumWithID(2).SetAmt(AmtStadt, 1);
            TestSpielwelt.GibBeweise(2, 9);

            var ergebnis = new ErpressungManager().FuehreErpressungDurch(2, false);

            Assert.False(ergebnis.Erfolg);
            Assert.True(ergebnis.BeweiseBleiben);
            Assert.Equal(9, SW.Dynamisch.GetAktHum().GetAktiveSpionage(2).GetDelikte());
        }

        [Fact]
        public void Mehrere_Erpressungen_laufen_parallel_und_enden_einzeln()
        {
            TestSpielwelt.Starte();
            var spieler = SW.Dynamisch.GetAktHum();
            int jahr = SW.Dynamisch.GetAktuellesJahr();

            spieler.ErpressungAnlegen(50, jahr);          // läuft dieses Jahr aus
            spieler.ErpressungAnlegen(51, jahr + 2);      // läuft weiter

            Assert.Equal(2, spieler.GetErpressungen().Count);

            // Im letzten Wirkungsjahr endet noch nichts.
            Assert.Empty(spieler.AbgelaufeneErpressungenEntfernen(jahr));

            var abgelaufen = spieler.AbgelaufeneErpressungenEntfernen(jahr + 1);

            Assert.Equal(new[] { 50 }, abgelaufen);
            Assert.True(spieler.ErpresstBereits(51));
            Assert.False(spieler.ErpresstBereits(50));
        }

        [Fact]
        public void Die_Beweisliste_nennt_die_tatsaechlichen_Vorwuerfe()
        {
            TestSpielwelt.Starte();
            int zielId = TestSpielwelt.SetzeKiGegner(0, AmtStadt);

            var ziel = SW.Dynamisch.GetSpWithID(zielId);
            for (int i = 0; i < SW.Statisch.GetMaxGesetze(); i++)
                ziel.SetBegingVerbrechenX(i, 0);

            ziel.SetBegingVerbrechenX(20, 1);   // Spionage

            var vorwuerfe = new ErpressungManager().GetBeweisliste(zielId);

            Assert.Single(vorwuerfe);
            Assert.Equal(SW.Statisch.GetGerichtsGesetzesvorwurf()[20], vorwuerfe[0]);
        }
    }
}
