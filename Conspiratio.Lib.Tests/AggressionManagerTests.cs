using System.Linq;

using Conspiratio.Lib.Allgemein;
using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>
    /// Aggressive KI (Issue): Die feindseligsten KIs sabotieren den aktiven Menschen oder schwärzen ihn
    /// an. Jede der höchstens <see cref="AggressionManager.MaxKandidatenProZug"/> Kandidatinnen feuert
    /// unabhängig und höchstens eine Aktion pro Zug.
    /// </summary>
    public class AggressionManagerTests
    {
        /// <summary>
        /// Reset all AI relationships to the human (100, i.e. as far from neutral-50 as possible) and
        /// Bosheit to 0, to isolate test cases from random default values. Both feed
        /// BerechneKiFeindseligkeitChance. Since the manager picks the most hostile KIs, a background KI
        /// with a randomly low default relationship would otherwise displace the KI a test deliberately
        /// made hostile - so the reset also pins *who* gets evaluated, not just how likely it is to fire.
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

        /// <summary>
        /// Neutralisiert zusätzlich alle KI-zu-KI-Beziehungen: über diese Achse sucht
        /// WaehleAnschwaerzenAdressat, nicht über die KI-zu-Mensch-Beziehung. Ohne das könnte eine der
        /// hunderten übrigen KIs zufällig eine bessere Beziehung zum Ankläger haben als der im Test
        /// absichtlich gewählte Adressat.
        /// </summary>
        private void ResetKiZuKiBeziehungen()
        {
            for (int i = SW.Statisch.GetMinKIID(); i < SW.Statisch.GetMaxKIID(); i++)
                for (int j = SW.Statisch.GetMinKIID(); j < SW.Statisch.GetMaxKIID(); j++)
                    SW.Dynamisch.GetKIwithID(i).SetBeziehungZuX(j, 50);
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
        public void Hoechstens_die_drei_feindseligsten_KIs_kommen_ueberhaupt_in_Frage()
        {
            // Sechs offen feindselige KIs; nur die drei feindseligsten duerfen ueberhaupt wuerfeln.
            // Genau diese Begrenzung haelt die Ereignisrate im Rahmen - ohne sie wuerfelt die
            // Kandidaten-Chance fuer jede der ~390 KIs einzeln und die Rate explodiert.
            TestSpielwelt.Starte(seed: 46);
            int menschId = SW.Dynamisch.GetAktiverSpieler();
            ResetKiRelationships(menschId);

            var kiIds = new int[6];
            for (int idx = 0; idx < 6; idx++)
            {
                kiIds[idx] = TestSpielwelt.SetzeKiGegner(idx, 0, bosheit: 0);
                SW.Dynamisch.GetKIwithID(kiIds[idx]).SetBeziehungZuX(menschId, -1000 + idx * 100);
            }

            var feindseligste = new[] { kiIds[0], kiIds[1], kiIds[2] };
            var mensch = SW.Dynamisch.GetAktHum();

            for (int versuch = 0; versuch < 50; versuch++)
            {
                foreach (int kiId in kiIds)
                    mensch.GegnerischeSabotageEntfernen(kiId);

                var ergebnisse = new AggressionManager().PruefeKiAggression(0);

                Assert.True(ergebnisse.Count <= AggressionManager.MaxKandidatenProZug,
                            $"{ergebnisse.Count} Aktionen in einem Zug, erlaubt sind hoechstens " +
                            AggressionManager.MaxKandidatenProZug);
                Assert.All(ergebnisse, e => Assert.Contains(e.TaeterId, feindseligste));
            }
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
        public void Ein_Zug_kann_Sabotage_und_Anschwaerzen_gemischt_liefern()
        {
            // Kontor.cs laeuft mit foreach ueber die Liste; die gemischte Liste ist also die Form, die
            // im echten Spiel auftritt, in der Testsuite aber sonst nirgends entsteht.
            TestSpielwelt.Starte(seed: 47);
            int menschId = SW.Dynamisch.GetAktiverSpieler();
            var mensch = SW.Dynamisch.GetAktHum();

            int anschwaerzer = TestSpielwelt.SetzeKiGegner(0, 0, bosheit: 0);
            int saboteur = TestSpielwelt.SetzeKiGegner(1, 0, bosheit: 0);
            ResetKiRelationships(menschId);
            ResetKiZuKiBeziehungen();
            SW.Dynamisch.GetKIwithID(anschwaerzer).SetBeziehungZuX(menschId, -1000);
            SW.Dynamisch.GetKIwithID(saboteur).SetBeziehungZuX(menschId, -1000);

            // Ein Adressat, der dem Anschwaerzer glaubt (Beziehung >= Schwelle 80) - und mit Amt, denn
            // angeschwaerzt wird nur bei Wuerdentraegern.
            int adressat = TestSpielwelt.SetzeKiGegner(2, 1, bosheit: 0);
            SW.Dynamisch.GetKIwithID(adressat).SetBeziehungZuX(menschId, 100);
            SW.Dynamisch.GetKIwithID(adressat).SetBeziehungZuX(anschwaerzer, 100);

            AggressionsErgebnis sabotage = null;
            AggressionsErgebnis anschwaerzen = null;

            for (int versuch = 0; versuch < 200 && (sabotage == null || anschwaerzen == null); versuch++)
            {
                // Laufende Sabotage beim einen erzwingt dessen Anschwaerzen, der andere waehlt frei.
                mensch.GetGegnerischeSabotage(anschwaerzer).SetDauer(3);
                mensch.GegnerischeSabotageEntfernen(saboteur);
                SW.Dynamisch.GetKIwithID(adressat).SetBeziehungZuX(anschwaerzer, 100); // Glaube senkt sie um 10

                var ergebnisse = new AggressionManager().PruefeKiAggression(0);

                if (ergebnisse.Any(e => e.Aktion == AggressionsAktion.Sabotage) &&
                    ergebnisse.Any(e => e.Aktion == AggressionsAktion.Anschwaerzen))
                {
                    sabotage = ergebnisse.First(e => e.Aktion == AggressionsAktion.Sabotage);
                    anschwaerzen = ergebnisse.First(e => e.Aktion == AggressionsAktion.Anschwaerzen);
                }
            }

            Assert.NotNull(sabotage);
            Assert.NotNull(anschwaerzen);
            Assert.NotEqual(sabotage.TaeterId, anschwaerzen.TaeterId);
            Assert.Null(sabotage.Meldung);       // Sabotage bleibt unbemerkt, erst die Wirkung meldet sich
            Assert.NotNull(anschwaerzen.Meldung);
        }

        [Fact]
        public void Die_Aggressionsrate_pro_Zug_bleibt_die_einer_seltenen_Zugmeldung()
        {
            // Regressionsschutz gegen den urspruenglichen Konstruktionsfehler: Die Chance aus
            // BerechneKiFeindseligkeitChance gilt je *Kandidat*. Wuerfelte man sie fuer jede der ~390 KIs
            // einzeln aus, kaeme das Hundertfache heraus - gemessen 4-14 Aktionen pro Zug, was den
            // Spieler wirtschaftlich ausloescht. Deshalb hier keine gepinnte Extremwelt, sondern ein
            // normales Spiel und eine grosse Stichprobe ueber die Gesamtrate.
            TestSpielwelt.Starte(seed: 4711);
            var manager = new AggressionManager();
            var mensch = SW.Dynamisch.GetAktHum();
            const int zuege = 2000;
            int aktionen = 0;

            for (int zug = 0; zug < zuege; zug++)
            {
                aktionen += manager.PruefeKiAggression(0).Count;

                // Zug abschliessen wie ZugNachrichtenManager.ErmittleGegnerischeSabotageNachrichten:
                // laufende Sabotagen altern, sonst saettigt sich der Zustand nach wenigen Zuegen.
                for (int i = SW.Statisch.GetMinKIID(); i < SW.Statisch.GetMaxKIID(); i++)
                {
                    var sabotage = mensch.GetGegnerischeSabotage(i);

                    if (sabotage.GetDauer() <= 0)
                        continue;

                    sabotage.ReduziereDauerUmEins();

                    if (sabotage.GetDauer() <= 0)
                        mensch.GegnerischeSabotageEntfernen(i);
                }
            }

            double rate = (double)aktionen / zuege;

            // Gemessen ueber mehrere Seeds: 0,14-0,26 Aktionen pro Zug, ohne Aufwaertsdrift ueber 1000
            // Zuege. Das Band laesst reichlich Luft nach oben und bleibt trotzdem eine Groessenordnung
            // unter dem alten Verhalten.
            Assert.InRange(rate, 0.05, 1.0);
        }

        [Fact]
        public void Realistische_Beziehung_0_mit_bosheit_0_produziert_etwa_6_prozent_chance()
        {
            // Die realistische Beziehungslage (0) muss rund 6 % Aggressionschance je Kandidat ergeben
            // (beziehung=0, bosheit=0 => feindseligkeit=50, chance=(50*13)/100=6,5%=6%). Der Test faengt
            // Regressionen wie den Guard ab, der das Feature dauerhaft unerreichbar machte.
            TestSpielwelt.Starte(seed: 48);
            int menschId = SW.Dynamisch.GetAktiverSpieler();
            var mensch = SW.Dynamisch.GetAktHum();

            // Genau so viele KIs feindselig setzen, wie pro Zug ueberhaupt gewuerfelt werden - dann ist
            // die Zahl der Wuerfe je Aufruf bekannt. Der Reset davor haelt die uebrigen ~390 KIs aus der
            // Kandidatenauswahl heraus, deren zufaellige Standardbeziehungen sonst mitzaehlen wuerden.
            ResetKiRelationships(menschId);
            ResetKiZuKiBeziehungen();

            var kiIds = new int[AggressionManager.MaxKandidatenProZug];
            for (int idx = 0; idx < kiIds.Length; idx++)
            {
                kiIds[idx] = TestSpielwelt.SetzeKiGegner(idx, 0, bosheit: 0);
                SW.Dynamisch.GetKIwithID(kiIds[idx]).SetBeziehungZuX(menschId, 0);
            }

            // Ein Adressat mit Amt, der jedem der drei glaubt: sonst versandet ein Teil der Wuerfe im
            // Anschwaerz-Zweig folgenlos und die gemessene Quote laege systematisch unter der Chance.
            int adressat = TestSpielwelt.SetzeKiGegner(kiIds.Length, 1, bosheit: 0);
            SW.Dynamisch.GetKIwithID(adressat).SetBeziehungZuX(menschId, 100);

            int aggressionen = 0;
            const int durchlaeufe = 1000;

            for (int iter = 0; iter < durchlaeufe; iter++)
            {
                foreach (int kiId in kiIds)
                {
                    mensch.GegnerischeSabotageEntfernen(kiId);
                    SW.Dynamisch.GetKIwithID(adressat).SetBeziehungZuX(kiId, 100); // Glaube senkt sie um 10
                }

                aggressionen += new AggressionManager().PruefeKiAggression(0).Count(e => kiIds.Contains(e.TaeterId));
            }

            double rate = (double)aggressionen / (kiIds.Length * durchlaeufe);
            Assert.True(rate >= 0.03 && rate <= 0.09,
                        $"Beobachtete Aggressionsquote {rate:P} ausserhalb des erwarteten 6-%-Bandes (3-9 %); " +
                        $"{aggressionen} Aktionen in {kiIds.Length * durchlaeufe} Wuerfen");
        }

        [Fact]
        public void Laeuft_bereits_eine_Sabotage_derselben_KI_wird_keine_zweite_ausgeloest_aber_angeschwaerzt()
        {
            TestSpielwelt.Starte(seed: 49);
            int menschId = SW.Dynamisch.GetAktiverSpieler();
            int kiId = TestSpielwelt.SetzeKiGegner(0, 0, bosheit: 0);
            TestSpielwelt.SetzeKiGegner(1, 1, bosheit: 0); // moeglicher Adressat, mit Amt

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
            TestSpielwelt.Starte(seed: 50);
            int menschId = SW.Dynamisch.GetAktiverSpieler();
            int anklaeger = TestSpielwelt.SetzeKiGegner(0, 0, bosheit: 0);
            int schlechterAdressat = TestSpielwelt.SetzeKiGegner(1, 1, bosheit: 0);
            int besterAdressat = TestSpielwelt.SetzeKiGegner(2, 2, bosheit: 0);

            // Reset erst NACH SetzeKiGegner (siehe Kommentar im Sabotage/Anschwaerzen-Test oben).
            ResetKiRelationships(menschId);
            ResetKiZuKiBeziehungen();

            SW.Dynamisch.GetKIwithID(anklaeger).SetBeziehungZuX(menschId, -1000);
            SW.Dynamisch.GetAktHum().GetGegnerischeSabotage(anklaeger).SetDauer(3); // erzwingt Anschwaerzen
            SW.Dynamisch.GetKIwithID(schlechterAdressat).SetBeziehungZuX(anklaeger, 10);
            SW.Dynamisch.GetKIwithID(besterAdressat).SetBeziehungZuX(anklaeger, 90);

            var ergebnisse = new AggressionManager().PruefeKiAggression(0);

            Assert.Single(ergebnisse);
            Assert.Contains(SW.Dynamisch.GetSpWithID(besterAdressat).GetKompletterName(), ergebnisse[0].Meldung);
        }

        [Fact]
        public void Angeschwaerzt_wird_nur_bei_KI_Amtstraegern()
        {
            // Der Feature-CHANGELOG verspricht "bei anderen Wuerdentraegern"; ohne Amt haette das
            // Anschwaerzen weder Gewicht noch Folgen.
            TestSpielwelt.Starte(seed: 51);
            int menschId = SW.Dynamisch.GetAktiverSpieler();
            int anklaeger = TestSpielwelt.SetzeKiGegner(0, 0, bosheit: 0);
            int ohneAmt = TestSpielwelt.SetzeKiGegner(1, 0, bosheit: 0);
            int mitAmt = TestSpielwelt.SetzeKiGegner(2, 1, bosheit: 0);

            ResetKiRelationships(menschId);
            ResetKiZuKiBeziehungen();

            SW.Dynamisch.GetKIwithID(anklaeger).SetBeziehungZuX(menschId, -1000);
            SW.Dynamisch.GetAktHum().GetGegnerischeSabotage(anklaeger).SetDauer(3); // erzwingt Anschwaerzen

            // Der Amtslose steht dem Anklaeger naeher - trotzdem muss der Amtstraeger gewaehlt werden.
            SW.Dynamisch.GetKIwithID(ohneAmt).SetBeziehungZuX(anklaeger, 100);
            SW.Dynamisch.GetKIwithID(mitAmt).SetBeziehungZuX(anklaeger, 90);

            var ergebnisse = new AggressionManager().PruefeKiAggression(0);

            Assert.Single(ergebnisse);
            Assert.Contains(SW.Dynamisch.GetSpWithID(mitAmt).GetKompletterName(), ergebnisse[0].Meldung);
            Assert.DoesNotContain(SW.Dynamisch.GetSpWithID(ohneAmt).GetKompletterName(), ergebnisse[0].Meldung);
        }

        [Fact]
        public void Die_Anschwaerz_Meldung_ist_aus_Sicht_des_angeschwaerzten_Menschen_geschrieben()
        {
            // AnschwaerzenAusfuehren liefert eine Meldung aus Sicht des Anklaegers ("Euren Worten"), weil
            // sie fuer den menschlichen Anklaeger im Hinterzimmer geschrieben ist. Hier ist der Mensch das
            // Opfer und hat mit niemandem gesprochen - er braucht seine eigene Meldung.
            TestSpielwelt.Starte(seed: 52);
            int menschId = SW.Dynamisch.GetAktiverSpieler();
            int anklaeger = TestSpielwelt.SetzeKiGegner(0, 0, bosheit: 0);
            int adressat = TestSpielwelt.SetzeKiGegner(1, 1, bosheit: 0);

            ResetKiRelationships(menschId);
            ResetKiZuKiBeziehungen();

            SW.Dynamisch.GetKIwithID(anklaeger).SetBeziehungZuX(menschId, -1000);
            SW.Dynamisch.GetAktHum().GetGegnerischeSabotage(anklaeger).SetDauer(3); // erzwingt Anschwaerzen
            SW.Dynamisch.GetKIwithID(adressat).SetBeziehungZuX(anklaeger, 90);

            var ergebnisse = new AggressionManager().PruefeKiAggression(0);

            Assert.Single(ergebnisse);
            string meldung = ergebnisse[0].Meldung;

            Assert.Contains(SW.Dynamisch.GetSpWithID(anklaeger).GetKompletterName(), meldung);
            Assert.Contains(SW.Dynamisch.GetSpWithID(adressat).GetKompletterName(), meldung);
            Assert.Contains("Euch", meldung);                  // der Mensch ist das Opfer
            Assert.Contains("angeschwärzt", meldung);
            Assert.Contains("Glauben", meldung);               // der Adressat glaubt es
            Assert.DoesNotContain("Euren Worten", meldung);    // nicht die Anklaeger-Perspektive
        }
    }
}
