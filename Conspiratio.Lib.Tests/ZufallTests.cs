using System.Collections.Generic;
using System.Linq;

using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>
    /// Prüft <see cref="StatischeSpieldaten.SetRnd"/>: Mit festem Startwert läuft ein Spiel
    /// reproduzierbar ab. Genau daran hing bisher, dass ein fehlgeschlagener automatischer Durchlauf
    /// nicht nachstellbar war.
    /// </summary>
    public class ZufallTests
    {
        [Fact]
        public void GleicherStartwertLiefertGleicheZahlenfolge()
        {
            SW.Statisch.Initialisieren();
            SW.Statisch.SetRnd(4711);
            var ersteFolge = ZieheZahlen(50);

            SW.Statisch.SetRnd(4711);
            var zweiteFolge = ZieheZahlen(50);

            Assert.Equal(ersteFolge, zweiteFolge);
        }

        [Fact]
        public void UnterschiedlicheStartwerteLiefernUnterschiedlicheFolgen()
        {
            SW.Statisch.Initialisieren();

            SW.Statisch.SetRnd(1);
            var ersteFolge = ZieheZahlen(50);

            SW.Statisch.SetRnd(2);
            var zweiteFolge = ZieheZahlen(50);

            Assert.NotEqual(ersteFolge, zweiteFolge);
        }

        /// <summary>
        /// Der eigentliche Zweck: Nicht die Zahlenfolge zählt, sondern dass ein kompletter Spielaufbau
        /// mit demselben Startwert denselben Zustand ergibt – die Bosheit der KI-Gegner (und damit ihre
        /// Stärke) und die verbleibenden Lebensjahre werden beim Start ausgewürfelt.
        /// </summary>
        [Fact]
        public void GleicherStartwertLiefertGleichenSpielaufbau()
        {
            TestSpielwelt.Starte(seed: 4711);
            var ersterAufbau = ErfasseAufbau();

            TestSpielwelt.Starte(seed: 4711);
            var zweiterAufbau = ErfasseAufbau();

            Assert.Equal(ersterAufbau, zweiterAufbau);
        }

        [Fact]
        public void OhneStartwertUnterscheidetSichDerSpielaufbau()
        {
            TestSpielwelt.Starte(seed: 4711);
            var mitStartwert = ErfasseAufbau();

            TestSpielwelt.Starte(seed: 1234);
            var mitAnderemStartwert = ErfasseAufbau();

            Assert.NotEqual(mitStartwert, mitAnderemStartwert);
        }

        private static List<int> ZieheZahlen(int anzahl)
            => Enumerable.Range(0, anzahl).Select(_ => SW.Statisch.Rnd.Next(1000)).ToList();

        /// <summary>Fasst die beim Spielstart ausgewürfelten Werte zu einem Vergleichswert zusammen.</summary>
        private static List<int> ErfasseAufbau()
        {
            var werte = new List<int> { SW.Dynamisch.GetHumWithID(1).GetVerbleibendeJahre() };

            // GetMaxKIID ist die Arraylänge, nicht die letzte gültige ID – daher "<".
            for (int id = SW.Statisch.GetMinKIID(); id < SW.Statisch.GetMaxKIID(); id++)
            {
                var ki = SW.Dynamisch.GetKIwithID(id);

                if (ki == null)
                    continue;

                werte.Add(ki.GetBosheit());
                werte.Add(ki.GetAlter());
            }

            return werte;
        }
    }
}
