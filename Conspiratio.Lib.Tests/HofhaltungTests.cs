using Conspiratio.Lib.Allgemein;
using Conspiratio.Lib.Gameplay.Spielwelt;
using Conspiratio.Lib.Gameplay.Titel;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>
    /// Der standesgemäße Aufwand: Ein Adelstitel kostet jährlich einen Anteil der Talerschwelle,
    /// ab der er verliehen wird. Er ist damit die erste laufende Ausgabe des Spiels, die nicht an der
    /// Aktivität hängt, sondern am Stand – und er trifft nur, wer überhaupt geadelt ist.
    /// Einsteiger sind Bürger und zahlen nichts.
    /// </summary>
    public class HofhaltungTests
    {
        private const int Herr = 0;
        private const int Buerger = 1;
        private const int Edelmann = 2;
        private const int Herzog = 9;

        /// <summary>Die beiden titellosen Ränge kosten nichts – sonst träfe es den Spielanfang.</summary>
        [Theory]
        [InlineData(Herr)]
        [InlineData(Buerger)]
        public void Ohne_Adelstitel_faellt_kein_Aufwand_an(int titelId)
        {
            TestSpielwelt.Starte();

            Assert.Equal(0, SW.Statisch.GetTitelX(titelId).GetJahresaufwand());
        }

        /// <summary>
        /// Der Aufwand leitet sich aus <see cref="Adelstitel.AbTaler"/> ab, damit Rang und Kosten nicht
        /// auseinanderlaufen können: Wer die Schwelle gerade hält, braucht
        /// <see cref="Adelstitel.AufwandProzent"/> Rendite, nur um sie zu halten.
        /// </summary>
        [Theory]
        [InlineData(Edelmann, 500)]
        [InlineData(Herzog, 50000)]
        public void Der_Aufwand_ist_ein_Zehntel_der_Talerschwelle(int titelId, int erwartet)
        {
            TestSpielwelt.Starte();

            var titel = SW.Statisch.GetTitelX(titelId);

            Assert.Equal(erwartet, titel.GetJahresaufwand());
            Assert.Equal(titel.AbTaler * Adelstitel.AufwandProzent / 100, titel.GetJahresaufwand());
        }

        /// <summary>Der Aufwand steigt mit jedem Rang – sonst wäre er keine Bremse.</summary>
        [Fact]
        public void Jeder_hoehere_Rang_kostet_mindestens_so_viel_wie_der_darunter()
        {
            TestSpielwelt.Starte();

            for (int titelId = 1; titelId < SW.Statisch.GetMaxTitelID(); titelId++)
            {
                Assert.True(SW.Statisch.GetTitelX(titelId).GetJahresaufwand() >=
                            SW.Statisch.GetTitelX(titelId - 1).GetJahresaufwand(),
                            "Rang " + titelId + " darf nicht billiger sein als Rang " + (titelId - 1) + ".");
            }
        }

        /// <summary>Die Jahresabrechnung weist den Aufwand aus und zieht ihn tatsächlich ab.</summary>
        [Fact]
        public void Die_Abrechnung_weist_den_Aufwand_aus_und_zieht_ihn_ab()
        {
            TestSpielwelt.Starte();

            var spieler = SW.Dynamisch.GetAktHum();
            spieler.SetTitel(Herzog);
            spieler.SetTaler(1000000);

            int vorher = spieler.GetTaler();
            var ergebnis = new AbrechnungsManager().ErstelleAbrechnungFuerAktivenSpieler();

            Assert.Equal(50000, ergebnis.Hofhaltung);
            Assert.True(ergebnis.Gesamtkosten >= ergebnis.Hofhaltung, "Der Aufwand muss in den Gesamtkosten stecken.");
            Assert.Equal(vorher - ergebnis.Gesamtkosten, spieler.GetTaler());
        }

        /// <summary>Ohne Titel bleibt die Abrechnung von dieser Position unberührt.</summary>
        [Fact]
        public void Ein_Buerger_zahlt_in_der_Abrechnung_keine_Hofhaltung()
        {
            TestSpielwelt.Starte();

            SW.Dynamisch.GetAktHum().SetTitel(Buerger);

            Assert.Equal(0, new AbrechnungsManager().ErstelleAbrechnungFuerAktivenSpieler().Hofhaltung);
        }

        /// <summary>
        /// Der Spieler waehlt die Stufe selbst. Gespeichert wird die Abweichung von der Mitte, damit die
        /// 0 aus einem alten Spielstand "standesgemaess" bedeutet - siehe den Savegame-Test unten.
        /// </summary>
        [Theory]
        [InlineData(-1, 50)]
        [InlineData(0, 100)]
        [InlineData(1, 200)]
        public void Die_Stufe_skaliert_den_standesgemaessen_Aufwand(int abweichung, int prozent)
        {
            TestSpielwelt.Starte();

            var spieler = SW.Dynamisch.GetAktHum();
            spieler.SetTitel(Herzog);
            spieler.SetTaler(1000000);
            spieler.SetHofhaltungAbweichung(abweichung);

            var ergebnis = new AbrechnungsManager().ErstelleAbrechnungFuerAktivenSpieler();

            Assert.Equal(50000 * prozent / 100, ergebnis.Hofhaltung);
        }

        /// <summary>
        /// Der Savegame-Test: Ein alter Stand kennt das Feld nicht, es kommt als 0 an - und 0 muss
        /// exakt das bisherige Verhalten bedeuten, sonst aendert sich fuer bestehende Spieler
        /// stillschweigend die Abrechnung.
        /// </summary>
        [Fact]
        public void Ein_alter_Spielstand_haelt_standesgemaess_Hof()
        {
            TestSpielwelt.Starte();

            var spieler = SW.Dynamisch.GetAktHum();
            spieler.SetTitel(Herzog);
            spieler.SetTaler(1000000);

            // Kein SetHofhaltungAbweichung: Das Feld steht auf seinem Standardwert, wie nach dem Laden.
            Assert.Equal(0, spieler.GetHofhaltungAbweichung());
            Assert.Equal(50000, new AbrechnungsManager().ErstelleAbrechnungFuerAktivenSpieler().Hofhaltung);
        }
    }
}
