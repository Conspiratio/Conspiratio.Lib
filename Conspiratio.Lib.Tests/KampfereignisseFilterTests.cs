using Conspiratio.Lib.Gameplay.Kampf;
using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>
    /// Der Filter der militärischen Jahresereignisse: Wer als "menschlich beteiligt" gilt, entscheidet
    /// darüber, ob eine Meldung trotz abgeschalteter KI-Militärereignisse erscheint.
    ///
    /// Hintergrund der Tests: Der Filter prüfte mit <c>id &lt;= GetMinKIID()</c>. Bei einem Überfall ohne
    /// Verteidiger ist die Verteidiger-ID aber 0 ("muss bei Überfällen nicht vorhanden sein"), und 0 ist
    /// kleiner als jede KI-ID – jeder wehrlose KI-Überfall galt damit als menschlich und wurde angezeigt.
    /// Zusätzlich zählte die KI mit genau <c>GetMinKIID()</c> als Mensch.
    /// </summary>
    public class KampfereignisseFilterTests
    {
        [Fact]
        public void Keine_Beteiligung_gilt_nicht_als_menschlicher_Spieler()
        {
            TestSpielwelt.Starte();

            // 0 bedeutet "kein Verteidiger" (wehrloser Überfall), nicht "Mensch Nummer 0".
            Assert.False(KampfereignisseManager.IstMenschlicherSpieler(0));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(9)]
        public void Menschliche_Spieler_ids_gelten_als_menschlich(int spielerId)
        {
            TestSpielwelt.Starte();

            Assert.True(KampfereignisseManager.IstMenschlicherSpieler(spielerId));
        }

        [Fact]
        public void Die_erste_KI_gilt_nicht_als_menschlich()
        {
            TestSpielwelt.Starte();

            Assert.False(KampfereignisseManager.IstMenschlicherSpieler(SW.Statisch.GetMinKIID()));
        }

        [Fact]
        public void Eine_spaetere_KI_gilt_nicht_als_menschlich()
        {
            TestSpielwelt.Starte();

            Assert.False(KampfereignisseManager.IstMenschlicherSpieler(SW.Statisch.GetMinKIID() + 1));
        }
    }
}
