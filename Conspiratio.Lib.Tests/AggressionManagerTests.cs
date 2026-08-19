using Conspiratio.Lib.Allgemein;
using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>
    /// Aggressive KI (Issue): KIs mit schlechter Beziehung zum aktiven Menschen sabotieren oder
    /// schwärzen ihn an. Jede KI feuert unabhängig höchstens eine Aktion pro Zug.
    /// </summary>
    public class AggressionManagerTests
    {
        [Fact]
        public void Extrem_schlechte_Beziehung_loest_eine_Aktion_aus()
        {
            TestSpielwelt.Starte();
            int menschId = SW.Dynamisch.GetAktiverSpieler();
            int kiId = TestSpielwelt.SetzeKiGegner(0, 0, bosheit: 0);
            SW.Dynamisch.GetKIwithID(kiId).SetBeziehungZuX(menschId, -1000);

            var ergebnisse = new AggressionManager().PruefeKiAggression(0);

            Assert.Single(ergebnisse);
            Assert.Equal(kiId, ergebnisse[0].TaeterId);
        }

        [Fact]
        public void Neutrale_Beziehung_loest_nichts_aus()
        {
            TestSpielwelt.Starte();
            TestSpielwelt.SetzeKiGegner(0, 0, bosheit: 0);

            Assert.Empty(new AggressionManager().PruefeKiAggression(0));
        }

        [Fact]
        public void Die_ausgenommene_KI_wird_uebersprungen()
        {
            TestSpielwelt.Starte();
            int menschId = SW.Dynamisch.GetAktiverSpieler();
            int kiId = TestSpielwelt.SetzeKiGegner(0, 0, bosheit: 0);
            SW.Dynamisch.GetKIwithID(kiId).SetBeziehungZuX(menschId, -1000);

            Assert.Empty(new AggressionManager().PruefeKiAggression(kiId));
        }

        [Fact]
        public void Mehrere_feindselige_KIs_koennen_unabhaengig_voneinander_feuern()
        {
            TestSpielwelt.Starte();
            int menschId = SW.Dynamisch.GetAktiverSpieler();
            int ki1 = TestSpielwelt.SetzeKiGegner(0, 0, bosheit: 0);
            int ki2 = TestSpielwelt.SetzeKiGegner(1, 0, bosheit: 0);
            SW.Dynamisch.GetKIwithID(ki1).SetBeziehungZuX(menschId, -1000);
            SW.Dynamisch.GetKIwithID(ki2).SetBeziehungZuX(menschId, -1000);

            var ergebnisse = new AggressionManager().PruefeKiAggression(0);

            Assert.Equal(2, ergebnisse.Count);
        }

        [Fact]
        public void Sabotage_setzt_die_Dauer_ohne_laufende_Kosten()
        {
            TestSpielwelt.Starte(seed: 1);
            int menschId = SW.Dynamisch.GetAktiverSpieler();
            var mensch = SW.Dynamisch.GetAktHum();
            int kiId = TestSpielwelt.SetzeKiGegner(0, 0, bosheit: 0);
            SW.Dynamisch.GetKIwithID(kiId).SetBeziehungZuX(menschId, -1000);

            bool sabotageBeobachtet = false;

            for (int versuch = 0; versuch < 200 && !sabotageBeobachtet; versuch++)
            {
                mensch.GegnerischeSabotageEntfernen(kiId);
                var ergebnisse = new AggressionManager().PruefeKiAggression(0);

                if (ergebnisse.Count == 1 && ergebnisse[0].Aktion == AggressionsAktion.Sabotage)
                {
                    sabotageBeobachtet = true;
                    Assert.Equal(AggressionManager.SabotageDauerJahre, mensch.GetGegnerischeSabotage(kiId).GetDauer());
                    Assert.Equal(0, mensch.GetGegnerischeSabotage(kiId).GetKosten());
                }
            }

            Assert.True(sabotageBeobachtet, "In 200 Versuchen kam kein Sabotage-Ergebnis vor.");
        }
    }
}
