using System.Linq;

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
        /// <summary>
        /// Reset all AI relationships to the human (100, i.e. as far from neutral-50 as possible) and
        /// Bosheit to 0, to isolate test cases from random default values. Both feed
        /// BerechneKiFeindseligkeitChance, so either alone can still trigger background noise from the
        /// hundreds of other KIs the game world creates. 100 rather than plain neutral (50) also survives
        /// a believed Anschwaerzen within the same PruefeKiAggression call: AnschwaerzenAusfuehren drops
        /// the addressee's own relationship to the human by 30 as a side effect, which at a 50 baseline
        /// can push a later-iterated background KI's feindseligkeit chance just above zero and cause it
        /// to independently fire too in the same turn - a real, if rare, in-turn cascade, not a bug.
        /// </summary>
        private void ResetKiRelationships(int menschId)
        {
            for (int i = SW.Statisch.GetMinKIID(); i < SW.Statisch.GetMaxKIID(); i++)
            {
                var ki = SW.Dynamisch.GetKIwithID(i);
                ki.SetBeziehungZuX(menschId, 100);
                ki.SetBosheit(0);
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
            ResetKiRelationships(menschId); // nach SetzeKiGegner, siehe Kommentar unten im Anschwaerzen-Test
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

                // Nur die 20 gezielt konfigurierten KIs zaehlen - der Rest der Spielwelt hat zufaellige
                // Standardbeziehungen (siehe TestSpielwelt.Starte) und wuerde die Quote sonst verfaelschen,
                // insbesondere seit eine laufende Sabotage im naechsten Zug zuverlaessig zum Anschwaerzen
                // fuehrt statt stillschweigend zu bleiben.
                var ergebnisse = new AggressionManager().PruefeKiAggression(0);
                aggressionCount += ergebnisse.Count(e => kiIds.Contains(e.TaeterId));
            }

            // Expected: 20 KIs * 500 iterations * ~0.06 chance = ~600 aggressions
            // Allow generous tolerance: ±3% (540-660)
            double observedRate = (double)aggressionCount / (20 * 500);
            Assert.True(observedRate >= 0.03 && observedRate <= 0.09,
                        $"Observed aggression rate {observedRate:P} outside expected 6% band (3-9%); " +
                        $"got {aggressionCount} aggressions in 10000 checks");
        }

        [Fact]
        public void Laeuft_bereits_eine_Sabotage_derselben_KI_wird_keine_zweite_ausgeloest_aber_angeschwaerzt()
        {
            TestSpielwelt.Starte();
            int menschId = SW.Dynamisch.GetAktiverSpieler();
            int kiId = TestSpielwelt.SetzeKiGegner(0, 0, bosheit: 0);
            TestSpielwelt.SetzeKiGegner(1, 0, bosheit: 0); // moeglicher Adressat

            // Reset erst NACH SetzeKiGegner: SetAmt kann fuer die neu vergebenen Aemter bestehende
            // KI-Amtsinhaber verdraengen, was deren Beziehung/Bosheit als Nebenwirkung veraendert - ein
            // Reset davor wuerde das nicht mehr erfassen.
            ResetKiRelationships(menschId);
            SW.Dynamisch.GetKIwithID(kiId).SetBeziehungZuX(menschId, -1000);
            SW.Dynamisch.GetAktHum().GetGegnerischeSabotage(kiId).SetDauer(3);

            var ergebnisse = new AggressionManager().PruefeKiAggression(0);

            Assert.Single(ergebnisse);
            Assert.Equal(AggressionsAktion.Anschwaerzen, ergebnisse[0].Aktion);
        }

        [Fact]
        public void Anschwaerzen_waehlt_die_KI_mit_der_besten_Beziehung_zum_Anklaeger_als_Adressat()
        {
            TestSpielwelt.Starte();
            int menschId = SW.Dynamisch.GetAktiverSpieler();
            int anklaeger = TestSpielwelt.SetzeKiGegner(0, 0, bosheit: 0);
            int schlechterAdressat = TestSpielwelt.SetzeKiGegner(1, 0, bosheit: 0);
            int besterAdressat = TestSpielwelt.SetzeKiGegner(2, 0, bosheit: 0);

            // Reset erst NACH SetzeKiGegner (siehe Kommentar im Sabotage/Anschwaerzen-Test oben), und
            // zusaetzlich alle KI-zu-KI-Beziehungen neutralisieren: WaehleAnschwaerzenAdressat sucht
            // ueber diese Achse, nicht die KI-zu-Mensch-Beziehung, die ResetKiRelationships abdeckt -
            // ohne diese zusaetzliche Neutralisierung koennte eine der hunderten anderen KIs zufaellig
            // eine hoehere Beziehung zum Anklaeger haben als der absichtlich gewaehlte Adressat.
            ResetKiRelationships(menschId);

            for (int i = SW.Statisch.GetMinKIID(); i < SW.Statisch.GetMaxKIID(); i++)
                for (int j = SW.Statisch.GetMinKIID(); j < SW.Statisch.GetMaxKIID(); j++)
                    SW.Dynamisch.GetKIwithID(i).SetBeziehungZuX(j, 50);

            SW.Dynamisch.GetKIwithID(anklaeger).SetBeziehungZuX(menschId, -1000);
            SW.Dynamisch.GetAktHum().GetGegnerischeSabotage(anklaeger).SetDauer(3); // erzwingt Anschwaerzen
            SW.Dynamisch.GetKIwithID(schlechterAdressat).SetBeziehungZuX(anklaeger, 10);
            SW.Dynamisch.GetKIwithID(besterAdressat).SetBeziehungZuX(anklaeger, 90);

            var ergebnisse = new AggressionManager().PruefeKiAggression(0);

            Assert.Single(ergebnisse);
            Assert.Contains(SW.Dynamisch.GetSpWithID(besterAdressat).GetKompletterName(), ergebnisse[0].Meldung);
        }
    }
}
