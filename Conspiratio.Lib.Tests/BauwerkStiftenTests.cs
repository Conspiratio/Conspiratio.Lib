using Conspiratio.Lib.Allgemein;
using Conspiratio.Lib.Gameplay.Privilegien.BauwerkStiften;
using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>
    /// Eine Bauwerksstiftung (5 000 Taler je Bauwerk) gab bisher ungedeckelt <c>Preis / 1000</c> Punkte
    /// permanentes Ansehen und liess sich im selben Zug beliebig oft wiederholen - 1 000 Taler je Punkt,
    /// die billigste Geltung im Spiel. Zwei Grenzen sperren das zu: Geltung aus Stiftungen ist wie die
    /// anderen beiden Quellen auf <see cref="AbrechnungsManager.AnsehenMaxProJahr"/> je Spieler und Jahr
    /// gedeckelt, und jedes der vier Bauwerke laesst sich je Stadt nur ein einziges Mal stiften.
    /// </summary>
    public class BauwerkStiftenTests
    {
        private const int Kirche = 0;      // "eine Kirche", 5 000 Taler
        private const int Kerker = 1;
        private const int Feuerwehr = 2;
        private const int Hospital = 3;

        /// <summary>
        /// Ohne Deckel bringen vier Stiftungen 4 * 5 = 20 Punkte. Der Deckel muss das auf
        /// <see cref="AbrechnungsManager.AnsehenMaxProJahr"/> (5) begrenzen - und zwar nur das Ansehen,
        /// nicht die Zahlung: Alle vier Stiftungen muessen bezahlt und gebaut sein. Gestiftet werden die
        /// vier *verschiedenen* Bauwerke derselben Stadt, denn dasselbe zweimal geht nicht mehr.
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
            int preis = manager.GetPreis(Kirche);

            foreach (int bauwerk in new[] { Kirche, Kerker, Feuerwehr, Hospital })
                manager.FuehreStiftungAus(bauwerk);

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
            manager.FuehreStiftungAus(Kirche);

            Assert.Equal(manager.GetPreis(Kirche) / 1000, spieler.GetPermaAnsehen() - ansehenVorher);
        }

        /// <summary>
        /// Der Deckel gilt je Jahr, nicht fuer immer: Ist er einmal ausgeschoepft, muss ein neues Jahr
        /// wieder Spielraum schaffen - sonst waere die Stiftung nach dem ersten Jahr fuer immer wertlos.
        /// Der Reset sitzt in <see cref="RundenManager.BeginneZug"/>, analog zum bestehenden Muster fuer
        /// <c>DuellGefuehrtDiesesJahr</c> ("Reset zu Zugbeginn"). Im neuen Jahr wird in der *naechsten*
        /// Stadt gestiftet, denn die vier Bauwerke der ersten sind vergeben.
        /// </summary>
        [Fact]
        public void Ein_neues_Jahr_schaltet_den_Deckel_wieder_frei()
        {
            TestSpielwelt.Starte();

            var spieler = SW.Dynamisch.GetAktHum();
            spieler.SetTaler(1000000);

            var manager = new BauwerkStiftenManager();

            // Deckel in diesem Jahr ausschoepfen (4 * 5 = 20 Punkte Rohgewinn gegen den Deckel von 5).
            foreach (int bauwerk in new[] { Kirche, Kerker, Feuerwehr, Hospital })
                manager.FuehreStiftungAus(bauwerk);

            Assert.Equal(AbrechnungsManager.AnsehenMaxProJahr, spieler.GetPermaAnsehen());

            // Einziger menschlicher Spieler: Zugende schaltet direkt das Jahr weiter.
            var rundenManager = new RundenManager();
            Assert.True(rundenManager.SchalteZumNaechstenSpieler(), "Der einzige Spieler muss das Jahr weiterschalten.");
            rundenManager.BeginneZug();

            int ansehenNachJahreswechsel = spieler.GetPermaAnsehen();
            manager.SetNextStadt();
            manager.FuehreStiftungAus(Kirche);

            Assert.Equal(manager.GetPreis(Kirche) / 1000, spieler.GetPermaAnsehen() - ansehenNachJahreswechsel);
        }

        /// <summary>
        /// Die zweite Grenze: Jedes der vier Bauwerke laesst sich je Stadt nur einmal stiften. Damit
        /// liegt die Lebensobergrenze bei 4 * 14 Staedten, und das Privileg wird zu etwas, das man im
        /// ganzen Reich verfolgt statt durch wiederholtes Klicken in derselben Stadt. Geprueft wird das
        /// vom Aufrufer ueber <c>IstBereitsGestiftet</c> samt Meldung - dasselbe Muster wie
        /// <c>KannBezahlen</c> / <c>GetNichtGenugGoldMeldung</c>.
        /// </summary>
        [Fact]
        public void Dasselbe_Bauwerk_laesst_sich_je_Stadt_nur_einmal_stiften()
        {
            TestSpielwelt.Starte();

            var spieler = SW.Dynamisch.GetAktHum();
            spieler.SetTaler(1000000);

            var manager = new BauwerkStiftenManager();
            int ersteStadt = manager.AktiveStadtID;

            Assert.False(manager.IstBereitsGestiftet(Kirche));

            manager.FuehreStiftungAus(Kirche);

            Assert.True(manager.IstBereitsGestiftet(Kirche));
            Assert.Contains(manager.GetStadtName(), manager.GetBereitsGestiftetMeldung(Kirche));

            // Ein anderes Bauwerk in derselben Stadt bleibt moeglich.
            Assert.False(manager.IstBereitsGestiftet(Kerker));

            // Und dasselbe Bauwerk in einer anderen Stadt ebenfalls.
            manager.SetNextStadt();
            Assert.NotEqual(ersteStadt, manager.AktiveStadtID);
            Assert.False(manager.IstBereitsGestiftet(Kirche));

            manager.FuehreStiftungAus(Kirche);
            Assert.True(manager.IstBereitsGestiftet(Kirche));

            // Zurueck in der ersten Stadt gilt die Sperre unveraendert weiter.
            while (manager.AktiveStadtID != ersteStadt)
                manager.SetNextStadt();

            Assert.True(manager.IstBereitsGestiftet(Kirche));
            Assert.False(manager.IstBereitsGestiftet(Hospital));
        }

        /// <summary>
        /// Die Serialisierung ist feldbasiert und umgeht Konstruktoren: Ein Spielstand aus der Zeit vor
        /// dieser Aenderung liefert die neue Liste als <c>null</c>, nicht als leere Liste. Das muss
        /// genau "noch nichts gestiftet" bedeuten und darf nirgends knallen - dafuer sorgt der
        /// Lazy-Init-Zugriff <c>HumSpieler.GetGestifteteBauwerke</c> nach dem Muster von
        /// <c>GetErpressungen</c> und <c>GetAhnentafelListe</c>.
        /// </summary>
        [Fact]
        public void Ein_alter_Spielstand_gilt_als_nichts_gestiftet()
        {
            TestSpielwelt.Starte();

            var spieler = SW.Dynamisch.GetAktHum();
            spieler.SetTaler(1000000);

            // So kommt das Feld aus einem alten Spielstand an.
            spieler.GestifteteBauwerke = null;

            var manager = new BauwerkStiftenManager();

            Assert.False(manager.IstBereitsGestiftet(Kirche));
            Assert.False(spieler.HatBauwerkGestiftet(manager.AktiveStadtID, Kirche));

            // Und ab hier zaehlt wieder normal mit.
            manager.FuehreStiftungAus(Kirche);
            Assert.True(manager.IstBereitsGestiftet(Kirche));
        }
    }
}
