using System.Linq;

using Conspiratio.Lib.Allgemein;
using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>
    /// Der Amtsenthebungsprozess am Jahresende (Migration von Main.AmtsenthebungenDurchfuehren): Zu jeder
    /// beantragten Enthebung stimmen bis zu drei Wähler ab; eine Mehrheit setzt das Opfer sofort ab.
    ///
    /// Bis hierher wurden Anträge zwar gestellt (Untergebenen-Ansicht, KI mit schlechter Beziehung), aber
    /// nie abgewickelt – sie verpufften folgenlos.
    /// </summary>
    public class AmtsenthebungTests
    {
        private const int Buergermeister = 1;
        private const int Gebiet = 1;

        [Fact]
        public void Ohne_Antrag_gibt_es_kein_Verfahren()
        {
            TestSpielwelt.Starte();

            Assert.Empty(new AmtsenthebungsManager().ErmittleVerfahren());
        }

        [Fact]
        public void Ein_Antrag_erzeugt_ein_Verfahren_mit_dem_Opfer()
        {
            TestSpielwelt.Starte();
            int opferId = TestSpielwelt.SetzeKiGegner(1, Buergermeister, gebietId: Gebiet);
            SW.Dynamisch.SetAmtsenthebungVonID(opferId);

            var verfahren = new AmtsenthebungsManager().ErmittleVerfahren();

            Assert.Single(verfahren);
            Assert.Equal(opferId, verfahren[0].OpferId);
            Assert.NotEmpty(verfahren[0].OpferName);
        }

        [Fact]
        public void Die_Waehlerliste_enthaelt_keine_Leerstellen()
        {
            TestSpielwelt.Starte();
            int opferId = TestSpielwelt.SetzeKiGegner(1, Buergermeister, gebietId: Gebiet);
            SW.Dynamisch.SetAmtsenthebungVonID(opferId);

            var verfahren = new AmtsenthebungsManager().ErmittleVerfahren()[0];

            Assert.All(verfahren.Waehler, w => Assert.NotEqual(0, w.SpielerId));
            Assert.All(verfahren.Waehler, w => Assert.NotEmpty(w.Name));
        }

        [Fact]
        public void Eine_KI_stimmt_bei_schlechter_Beziehung_zum_Opfer_fuer_die_Absetzung()
        {
            TestSpielwelt.Starte();
            var manager = new AmtsenthebungsManager();
            int opferId = TestSpielwelt.SetzeKiGegner(1, Buergermeister, gebietId: Gebiet);
            SW.Dynamisch.SetAmtsenthebungVonID(opferId);

            var verfahren = manager.ErmittleVerfahren()[0];
            SetzeBeziehungAllerWaehlerZumOpfer(verfahren, opferId, beziehung: 5);

            manager.ErgaenzeKiStimmen(verfahren);

            Assert.All(verfahren.Waehler.Where(w => !w.IstMensch), w => Assert.True(w.StimmtDafuer));
        }

        [Fact]
        public void Eine_KI_stimmt_bei_guter_Beziehung_zum_Opfer_gegen_die_Absetzung()
        {
            TestSpielwelt.Starte();
            var manager = new AmtsenthebungsManager();
            int opferId = TestSpielwelt.SetzeKiGegner(1, Buergermeister, gebietId: Gebiet);
            SW.Dynamisch.SetAmtsenthebungVonID(opferId);

            var verfahren = manager.ErmittleVerfahren()[0];
            SetzeBeziehungAllerWaehlerZumOpfer(verfahren, opferId, beziehung: 90);

            manager.ErgaenzeKiStimmen(verfahren);

            Assert.All(verfahren.Waehler.Where(w => !w.IstMensch), w => Assert.False(w.StimmtDafuer));
        }

        [Fact]
        public void Eine_Mehrheit_setzt_das_Opfer_ab()
        {
            TestSpielwelt.Starte();
            var manager = new AmtsenthebungsManager();
            int opferId = TestSpielwelt.SetzeKiGegner(1, Buergermeister, gebietId: Gebiet);
            SW.Dynamisch.SetAmtsenthebungVonID(opferId);

            var verfahren = manager.ErmittleVerfahren()[0];
            foreach (var waehler in verfahren.Waehler)
                waehler.StimmtDafuer = true;

            var ergebnis = manager.WerteAus(verfahren);

            Assert.True(ergebnis.Abgesetzt);
            Assert.Equal(0, SW.Dynamisch.GetSpWithID(opferId).GetAmtID());
        }

        [Fact]
        public void Ohne_Mehrheit_behaelt_das_Opfer_sein_Amt()
        {
            TestSpielwelt.Starte();
            var manager = new AmtsenthebungsManager();
            int opferId = TestSpielwelt.SetzeKiGegner(1, Buergermeister, gebietId: Gebiet);
            SW.Dynamisch.SetAmtsenthebungVonID(opferId);

            var verfahren = manager.ErmittleVerfahren()[0];
            foreach (var waehler in verfahren.Waehler)
                waehler.StimmtDafuer = false;

            var ergebnis = manager.WerteAus(verfahren);

            Assert.False(ergebnis.Abgesetzt);
            Assert.Equal(Buergermeister, SW.Dynamisch.GetSpWithID(opferId).GetAmtID());
        }

        [Fact]
        public void Ein_ausgewertetes_Verfahren_taucht_nicht_erneut_auf()
        {
            TestSpielwelt.Starte();
            var manager = new AmtsenthebungsManager();
            int opferId = TestSpielwelt.SetzeKiGegner(1, Buergermeister, gebietId: Gebiet);
            SW.Dynamisch.SetAmtsenthebungVonID(opferId);

            manager.WerteAus(manager.ErmittleVerfahren()[0]);

            Assert.Empty(manager.ErmittleVerfahren());
        }

        private static void SetzeBeziehungAllerWaehlerZumOpfer(AmtsenthebungsVerfahren verfahren, int opferId, int beziehung)
        {
            foreach (var waehler in verfahren.Waehler.Where(w => !w.IstMensch))
            {
                var ki = SW.Dynamisch.GetKIwithID(waehler.SpielerId);
                ki.ErhoeheBeziehungZuX(opferId, beziehung - ki.GetBeziehungZuKIX(opferId));
            }
        }
    }
}
