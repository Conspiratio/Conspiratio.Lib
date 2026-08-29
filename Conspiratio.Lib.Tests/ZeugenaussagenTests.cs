using System.Collections.Generic;

using Conspiratio.Lib.Gameplay.Justiz;
using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>
    /// Die Zeugenaussagen einer Verhandlung. Beide Zeugen stehen haeufig auf derselben Seite und in
    /// derselben Staerke und greifen dann auf denselben Topf von drei bis vier Zitaten zu - ohne
    /// Entprellung sagten sie hintereinander mit rund einem Drittel Wahrscheinlichkeit woertlich
    /// dasselbe. Geprueft wird ueber viele Wuerfe, nicht an einem Einzelfall: Ein Fehler dieser Art
    /// zeigt sich erst in der Rate.
    /// </summary>
    public class ZeugenaussagenTests
    {
        /// <summary>Der menschliche Spieler, den TestSpielwelt anlegt.</summary>
        private const int SpielerId = 1;

        /// <summary>
        /// Baut eine Verhandlung, in der der Mensch angeklagt ist und eine KI klagt. Die beiden Zeugen
        /// waehlt der Manager selbst (die ersten beiden unbeteiligten KIs); ihre Beziehungen werden
        /// gleichgesetzt, damit beide auf derselben Seite und in derselben Staerke aussagen - genau der
        /// Fall, in dem sich eine Wiederholung zeigen kann.
        /// </summary>
        private static GerichtsverhandlungManager BaueVerhandlung()
        {
            int klaeger = TestSpielwelt.SetzeKiGegner(1, 0);
            int richter1 = TestSpielwelt.SetzeKiGegner(2, 0);
            int richter2 = TestSpielwelt.SetzeKiGegner(3, 0);
            int richter3 = TestSpielwelt.SetzeKiGegner(4, 0);

            var verhandlung = SW.Dynamisch.GetGerichtsverhandlungX(0);
            verhandlung.SetAll(richter1, richter2, richter3, 1, 1, SpielerId, klaeger);

            var manager = new GerichtsverhandlungManager();
            manager.StarteVerhandlung(0);

            // Alle KIs gleichstellen, statt die Zeugen-IDs zu erraten: Welche zwei der Manager waehlt,
            // haengt daran, welche IDs Partei oder Richter sind. Bei Beziehung 0 zu beiden Seiten sagt
            // jeder Zeuge fuer den Angeklagten und schwach aus - beide greifen also in denselben
            // Zitat-Topf, genau der Fall, in dem sich eine Wiederholung zeigen kann.
            for (int id = SW.Statisch.GetMinKIID(); id < SW.Statisch.GetMaxKIID(); id++)
            {
                var ki = SW.Dynamisch.GetKIwithID(id);

                if (ki == null)
                    continue;

                ki.SetBeziehungZuX(SpielerId, 0);
                ki.SetBeziehungZuX(klaeger, 0);
            }

            return manager;
        }

        /// <summary>
        /// Der Wortlaut einer Aussage ohne die Namenszeile. Nur darauf kommt es an: Die Namen der beiden
        /// Zeugen sind ohnehin verschieden, ein Vergleich der ganzen Texte ginge deshalb immer gut aus
        /// und pruefte nichts.
        /// </summary>
        private static string Wortlaut(ZeugenAussage aussage)
        {
            string text = aussage.Text;
            int zeilenende = text.IndexOf((char)10);

            return zeilenende < 0 ? text : text.Substring(zeilenende + 1);
        }

        [Fact]
        public void Zwei_Zeugen_sagen_nie_hintereinander_dasselbe()
        {
            TestSpielwelt.Starte(seed: 4711);
            var manager = BaueVerhandlung();

            Assert.True(manager.GetZeugenAnzahl() >= 2, "Der Test braucht mindestens zwei Zeugen.");

            for (int runde = 0; runde < 2000; runde++)
            {
                List<ZeugenAussage> aussagen = manager.ErmittleZeugenAussagen();

                for (int i = 1; i < aussagen.Count; i++)
                {
                    Assert.False(Wortlaut(aussagen[i]) == Wortlaut(aussagen[i - 1]),
                        "Zwei Zeugen nacheinander mit demselben Wortlaut in Runde " + runde + ": " + Wortlaut(aussagen[i]));
                }
            }
        }

        /// <summary>
        /// Die Entprellung darf die Zitate nicht auf zwei feste Saetze einfrieren: Ueber viele Wuerfe
        /// muessen weiterhin alle Zitate des jeweiligen Topfes vorkommen. Sonst waere der Fehler nur
        /// gegen Eintoenigkeit anderer Art getauscht.
        /// </summary>
        [Fact]
        public void Die_Entprellung_nimmt_der_Auswahl_nicht_die_Vielfalt()
        {
            TestSpielwelt.Starte(seed: 4711);
            var manager = BaueVerhandlung();

            var gesehen = new HashSet<string>();

            for (int runde = 0; runde < 2000; runde++)
                foreach (var aussage in manager.ErmittleZeugenAussagen())
                    gesehen.Add(Wortlaut(aussage));

            // Der Topf fuer diese Seite und Staerke haelt drei Zitate; alle drei muessen vorkommen.
            Assert.Equal(3, gesehen.Count);
        }
    }
}
