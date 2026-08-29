using Conspiratio.Lib.Allgemein;
using Conspiratio.Lib.Gameplay.Privilegien.BauwerkStiften;
using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>
    /// Eine Bauwerksstiftung (5 000 Taler je Bauwerk) gab bisher ungedeckelt <c>Preis / 1000</c> Punkte
    /// permanentes Ansehen und liess sich im selben Zug beliebig oft wiederholen - 1 000 Taler je Punkt,
    /// die billigste Geltung im Spiel. Dieser Test sperrt das wieder zu: Geltung aus Stiftungen ist jetzt
    /// wie die anderen beiden Quellen auf <see cref="AbrechnungsManager.AnsehenMaxProJahr"/> je Spieler
    /// und Jahr gedeckelt, waehrend Taler und Bauwert weiterhin voll gezaehlt werden.
    /// </summary>
    public class BauwerkStiftenTests
    {
        private const int Index = 0; // "eine Kirche", 5 000 Taler

        /// <summary>
        /// Ohne Deckel bringen vier Stiftungen 4 * 5 = 20 Punkte. Der Deckel muss das auf
        /// <see cref="AbrechnungsManager.AnsehenMaxProJahr"/> (5) begrenzen - und zwar nur das Ansehen,
        /// nicht die Zahlung: Alle vier Stiftungen muessen bezahlt und gebaut sein.
        /// </summary>
        [Fact]
        public void Ansehen_aus_Stiftungen_ist_auf_das_Jahresmaximum_gedeckelt()
        {
            TestSpielwelt.Starte();

            var spieler = SW.Dynamisch.GetAktHum();
            spieler.SetTaler(1000000);
            int talerVorher = spieler.GetTaler();
            int ansehenVorher = spieler.GetPermaAnsehen();

            var manager = new BauwerkStiftenManager();
            int preis = manager.GetPreis(Index);

            for (int i = 0; i < 4; i++)
                manager.FuehreStiftungAus(Index);

            Assert.Equal(AbrechnungsManager.AnsehenMaxProJahr, spieler.GetPermaAnsehen() - ansehenVorher);

            // Die Taler sind trotzdem voll abgezogen - der Deckel bremst nur das Ansehen.
            Assert.Equal(talerVorher - 4 * preis, spieler.GetTaler());

            // Und der gestiftete Gesamtwert (Auftrag "Maezen") zaehlt ebenfalls ungedeckelt weiter.
            Assert.Equal(4 * preis, spieler.GestifteterBauwert);
        }

        /// <summary>Eine einzelne Stiftung unterhalb des Deckels bringt weiterhin die vollen Punkte.</summary>
        [Fact]
        public void Eine_einzelne_Stiftung_bringt_die_vollen_Punkte()
        {
            TestSpielwelt.Starte();

            var spieler = SW.Dynamisch.GetAktHum();
            spieler.SetTaler(1000000);
            int ansehenVorher = spieler.GetPermaAnsehen();

            var manager = new BauwerkStiftenManager();
            manager.FuehreStiftungAus(Index);

            Assert.Equal(manager.GetPreis(Index) / 1000, spieler.GetPermaAnsehen() - ansehenVorher);
        }

        /// <summary>
        /// Der Deckel gilt je Jahr, nicht fuer immer: Ist er einmal ausgeschoepft, muss ein neues Jahr
        /// wieder Spielraum schaffen - sonst waere die Stiftung nach dem ersten Jahr fuer immer wertlos.
        /// Der Reset sitzt in <see cref="RundenManager.BeginneZug"/>, analog zum bestehenden Muster fuer
        /// <c>DuellGefuehrtDiesesJahr</c> ("Reset zu Zugbeginn").
        /// </summary>
        [Fact]
        public void Ein_neues_Jahr_schaltet_den_Deckel_wieder_frei()
        {
            TestSpielwelt.Starte();

            var spieler = SW.Dynamisch.GetAktHum();
            spieler.SetTaler(1000000);

            var manager = new BauwerkStiftenManager();

            // Deckel in diesem Jahr ausschoepfen (4 * 5 = 20 Punkte Rohgewinn gegen den Deckel von 5).
            for (int i = 0; i < 4; i++)
                manager.FuehreStiftungAus(Index);

            Assert.Equal(AbrechnungsManager.AnsehenMaxProJahr, spieler.GetPermaAnsehen());

            // Einziger menschlicher Spieler: Zugende schaltet direkt das Jahr weiter.
            var rundenManager = new RundenManager();
            Assert.True(rundenManager.SchalteZumNaechstenSpieler(), "Der einzige Spieler muss das Jahr weiterschalten.");
            rundenManager.BeginneZug();

            int ansehenNachJahreswechsel = spieler.GetPermaAnsehen();
            manager.FuehreStiftungAus(Index);

            Assert.Equal(manager.GetPreis(Index) / 1000, spieler.GetPermaAnsehen() - ansehenNachJahreswechsel);
        }
    }
}
