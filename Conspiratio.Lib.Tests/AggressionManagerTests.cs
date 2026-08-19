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
        /// <summary>Reset all AI relationships to neutral (50) to isolate test cases from random default values.</summary>
        private void ResetKiRelationships(int menschId)
        {
            for (int i = SW.Statisch.GetMinKIID(); i < SW.Statisch.GetMaxKIID(); i++)
            {
                var ki = SW.Dynamisch.GetKIwithID(i);
                ki.SetBeziehungZuX(menschId, 50);
            }
        }

        [Fact]
        public void Extrem_schlechte_Beziehung_loest_eine_Aktion_aus()
        {
            TestSpielwelt.Starte(seed: 42);
            int menschId = SW.Dynamisch.GetAktiverSpieler();
            ResetKiRelationships(menschId);
            int kiId = TestSpielwelt.SetzeKiGegner(0, 0, bosheit: 0);
            SW.Dynamisch.GetKIwithID(kiId).SetBeziehungZuX(menschId, -1000);

            var ergebnisse = new AggressionManager().PruefeKiAggression(0);

            Assert.Single(ergebnisse);
            Assert.Equal(kiId, ergebnisse[0].TaeterId);
        }

        [Fact]
        public void Neutrale_Beziehung_loest_nichts_aus()
        {
            TestSpielwelt.Starte(seed: 43);
            int menschId = SW.Dynamisch.GetAktiverSpieler();
            ResetKiRelationships(menschId);
            TestSpielwelt.SetzeKiGegner(0, 0, bosheit: 0);

            Assert.Empty(new AggressionManager().PruefeKiAggression(0));
        }

        [Fact]
        public void Die_ausgenommene_KI_wird_uebersprungen()
        {
            TestSpielwelt.Starte(seed: 44);
            int menschId = SW.Dynamisch.GetAktiverSpieler();
            ResetKiRelationships(menschId);
            int kiId = TestSpielwelt.SetzeKiGegner(0, 0, bosheit: 0);
            SW.Dynamisch.GetKIwithID(kiId).SetBeziehungZuX(menschId, -1000);

            Assert.Empty(new AggressionManager().PruefeKiAggression(kiId));
        }

        [Fact]
        public void Mehrere_feindselige_KIs_koennen_unabhaengig_voneinander_feuern()
        {
            TestSpielwelt.Starte(seed: 45);
            int menschId = SW.Dynamisch.GetAktiverSpieler();
            ResetKiRelationships(menschId);
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

        [Fact]
        public void Realistische_Beziehung_0_mit_bosheit_0_produziert_etwa_6_prozent_chance()
        {
            // Verifies that the realistic relationship=0 band produces approximately 6% aggression chance
            // (beziehung=0, bosheit=0 => feindseligkeit=50, chance=(50*13)/100=6.5%=6%)
            // This test catches regressions like the guard that made the feature permanently unreachable.

            TestSpielwelt.Starte();
            int menschId = SW.Dynamisch.GetAktiverSpieler();
            var mensch = SW.Dynamisch.GetAktHum();

            // Set up 20 KIs with realistic default relationships (0) and bosheit (0)
            var kiIds = new int[20];
            for (int idx = 0; idx < 20; idx++)
            {
                kiIds[idx] = TestSpielwelt.SetzeKiGegner(idx, 0, bosheit: 0);
                // Explicitly set relationships to 0 (realistic value, not the random default [20-80])
                SW.Dynamisch.GetKIwithID(kiIds[idx]).SetBeziehungZuX(menschId, 0);
            }

            // Run 500 iterations, each checking if aggression happens
            int aggressionCount = 0;
            for (int iter = 0; iter < 500; iter++)
            {
                // Clear all sabotages before each check
                foreach (int kiId in kiIds)
                    mensch.GegnerischeSabotageEntfernen(kiId);

                var ergebnisse = new AggressionManager().PruefeKiAggression(0);
                aggressionCount += ergebnisse.Count;
            }

            // Expected: 20 KIs * 500 iterations * ~0.06 chance = ~600 aggressions
            // Allow generous tolerance: ±3% (540-660)
            double observedRate = (double)aggressionCount / (20 * 500);
            Assert.True(observedRate >= 0.03 && observedRate <= 0.09,
                        $"Observed aggression rate {observedRate:P} outside expected 6% band (3-9%); " +
                        $"got {aggressionCount} aggressions in 10000 checks");
        }
    }
}
