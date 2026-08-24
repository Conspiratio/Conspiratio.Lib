using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>
    /// Balancing der Bestechung: Taler sollen Beziehungen kaufen koennen, aber nicht den ganzen Hof
    /// in einer einzigen Zahlung. Geprueft wird beides - dass eine Bestechung genau einmal wirkt
    /// (und nicht als Dauerrente stehen bleibt) und dass ihr Jahresertrag gedeckelt ist.
    /// </summary>
    public class BestechungTests
    {
        /// <summary>Der menschliche Spieler, den TestSpielwelt anlegt.</summary>
        private const int SpielerId = 1;

        /// <summary>Eine KI ohne Amt - der mit Abstand haeufigste Fall unter den 390 Gegenspielern.</summary>
        private static int SetzeAmtloseKi()
        {
            int kiId = TestSpielwelt.SetzeKiGegner(1, 0);
            SW.Dynamisch.GetKIwithID(kiId).SetBeziehungZuX(SpielerId, 0);
            return kiId;
        }

        [Fact]
        public void Eine_Bestechung_verbessert_die_Beziehung()
        {
            TestSpielwelt.Starte();
            int kiId = SetzeAmtloseKi();

            SW.Dynamisch.Bestechen(kiId, 50);
            SW.Dynamisch.RundenBestechungenAbwickeln();

            // Amtlos, also ungeteilt: 50 Taler / 10 = 5 Punkte.
            Assert.Equal(5, SW.Dynamisch.GetKIwithID(kiId).GetBeziehungZuKIX(SpielerId));
        }

        /// <summary>
        /// Der Eintrag muss nach der Auszahlung geloescht sein. Wurde er stehen gelassen, zahlte das
        /// Rundenende dieselbe Bestechung Jahr fuer Jahr erneut aus - eine Dauerrente fuer die KI und
        /// eine Beziehung, die ohne weitere Zahlung bis zum Maximum weiterstieg.
        /// </summary>
        [Fact]
        public void Eine_Bestechung_wirkt_nur_im_Jahr_ihrer_Zahlung()
        {
            TestSpielwelt.Starte();
            int kiId = SetzeAmtloseKi();

            SW.Dynamisch.Bestechen(kiId, 50);
            SW.Dynamisch.RundenBestechungenAbwickeln();

            int beziehungNachZahlung = SW.Dynamisch.GetKIwithID(kiId).GetBeziehungZuKIX(SpielerId);
            int talerNachZahlung = SW.Dynamisch.GetKIwithID(kiId).GetTaler();

            SW.Dynamisch.RundenBestechungenAbwickeln();   // Folgejahr, ohne neue Zahlung

            Assert.Equal(beziehungNachZahlung, SW.Dynamisch.GetKIwithID(kiId).GetBeziehungZuKIX(SpielerId));
            Assert.Equal(talerNachZahlung, SW.Dynamisch.GetKIwithID(kiId).GetTaler());
        }

        /// <summary>
        /// Ohne Deckel brachten 1 000 Taler an eine amtlose KI +100 Beziehungspunkte, also sofort das
        /// Maximum. Mit Deckel bleibt der Kauf moeglich, dauert aber Jahre.
        /// </summary>
        [Fact]
        public void Grosse_Summen_kaufen_hoechstens_zehn_Punkte_im_Jahr()
        {
            TestSpielwelt.Starte();
            int kiId = SetzeAmtloseKi();
            SW.Dynamisch.GetAktHum().ErhoeheTaler(1000000);

            SW.Dynamisch.Bestechen(kiId, 1000000);
            SW.Dynamisch.RundenBestechungenAbwickeln();

            Assert.Equal(10, SW.Dynamisch.GetKIwithID(kiId).GetBeziehungZuKIX(SpielerId));
        }

        /// <summary>Die volle Summe wechselt trotz gedeckelter Wirkung den Besitzer.</summary>
        [Fact]
        public void Der_Deckel_verschluckt_keine_Taler()
        {
            TestSpielwelt.Starte();
            int kiId = SetzeAmtloseKi();
            SW.Dynamisch.GetAktHum().ErhoeheTaler(1000000);

            int talerVorher = SW.Dynamisch.GetKIwithID(kiId).GetTaler();

            SW.Dynamisch.Bestechen(kiId, 1000000);
            SW.Dynamisch.RundenBestechungenAbwickeln();

            Assert.Equal(talerVorher + 1000000, SW.Dynamisch.GetKIwithID(kiId).GetTaler());
        }

        /// <summary>
        /// Der Deckel darf kleine Bestechungen nicht anfassen - sie sind das Werkzeug der ersten Jahre.
        /// </summary>
        [Theory]
        [InlineData(10, 1)]
        [InlineData(50, 5)]
        [InlineData(90, 9)]
        public void Kleine_Bestechungen_wirken_unveraendert(int summe, int erwarteterGewinn)
        {
            TestSpielwelt.Starte();
            int kiId = SetzeAmtloseKi();

            SW.Dynamisch.Bestechen(kiId, summe);
            SW.Dynamisch.RundenBestechungenAbwickeln();

            Assert.Equal(erwarteterGewinn, SW.Dynamisch.GetKIwithID(kiId).GetBeziehungZuKIX(SpielerId));
        }
    }
}
