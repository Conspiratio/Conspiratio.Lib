using Conspiratio.Lib.Allgemein;
using Conspiratio.Lib.Gameplay.Justiz;
using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>
    /// Das Standesansehen (<c>Spieler.GetStandesAnsehen</c>) ist das Ansehen zuzüglich des
    /// Ansehensbonus des geführten Adelstitels. Es gilt ausdrücklich nur dort, wo über eine
    /// *Person* geurteilt wird – vor Gericht und vor den Geschworenen des Schuldenprozesses.
    ///
    /// Ebenso ausdrücklich gilt es *nicht* in der Wahl: Titel hängen an Talerschwellen, ein
    /// Titelbonus im Wahlbonus würde also den gerade erst geschlossenen Weg von Geld zu Einfluss
    /// wieder öffnen. Die Tests hier pinnen beide Seiten – die neue Wirkung und die bewusst
    /// unveränderten Stellen.
    /// </summary>
    public class StandesAnsehenTests
    {
        private const int Herr = 0;        // BonusAnsehen 0
        private const int Landherr = 4;    // BonusAnsehen 35
        private const int Graf = 7;        // BonusAnsehen 100
        private const int Herzog = 9;      // BonusAnsehen 150

        // Umlautfreie Textmarken der drei Verteidigungsplädoyers (GetVerteidigungsplaedoyer).
        private const string PlaedoyerHoch = "makellose Ansehen";
        private const string PlaedoyerMittel = "gebt dem guten Ruf";
        private const string PlaedoyerNiedrig = "beteuert die Unschuld";

        /// <summary>
        /// Führt eine Verhandlung gegen den angegebenen Angeklagten bis zum Verteidigungsplädoyer und
        /// liefert dessen Text. Der Text folgt denselben Schwellen wie der Plädoyerbonus, ist aber im
        /// Gegensatz zu jenem von außen beobachtbar.
        /// </summary>
        private static string Verteidigungsplaedoyer(int angeklagterId, int klaegerId)
        {
            var verhandlung = SW.Dynamisch.GetGerichtsverhandlungX(0);
            verhandlung.SetAngeklagterID(angeklagterId);
            verhandlung.SetKlaegerID(klaegerId);
            verhandlung.SetGebietsID(1);
            verhandlung.SetGebietsStufe(0);

            int richter = SW.Statisch.GetMinKIID() + 20;

            for (int i = 0; i < GerichtsverhandlungManager.RichterAnzahl; i++)
                verhandlung.SetRichterXID(i, richter + i);

            var manager = new GerichtsverhandlungManager();
            manager.StarteVerhandlung(0);

            return manager.GetVerteidigungsplaedoyer();
        }

        /// <summary>
        /// Der Kern der Änderung: Die Gerichtsschwellen (80 / 30) liegen genau zwischen den Rängen der
        /// Titelleiter. Ein Graf (Bonus 100) erreicht die hohe Schwelle allein durch seinen Stand, ein
        /// Landherr (35) die mittlere – ein titelloser Spieler mit demselben Ansehen keine von beiden.
        /// </summary>
        [Fact]
        public void Der_Titel_traegt_vor_Gericht()
        {
            TestSpielwelt.Starte();

            var spieler = SW.Dynamisch.GetAktHum();
            spieler.SetAnsehen(0);
            int klaeger = SW.Statisch.GetMinKIID() + 1;

            spieler.SetTitel(Herr);
            Assert.Equal(0, spieler.GetStandesAnsehen());
            Assert.Contains(PlaedoyerNiedrig, Verteidigungsplaedoyer(1, klaeger));

            spieler.SetTitel(Landherr);
            Assert.Equal(35, spieler.GetStandesAnsehen());
            Assert.Contains(PlaedoyerMittel, Verteidigungsplaedoyer(1, klaeger));

            spieler.SetTitel(Graf);
            Assert.Equal(100, spieler.GetStandesAnsehen());
            Assert.Contains(PlaedoyerHoch, Verteidigungsplaedoyer(1, klaeger));
        }

        /// <summary>
        /// Der eigentliche Grund, das Standesansehen auf <c>Spieler</c> und nicht in
        /// <c>HumSpieler.AnsehenAktualisieren</c> anzusiedeln: Ein KI-Angeklagter muss vor Gericht
        /// genauso von seinem Titel profitieren wie ein Mensch. Genau diese Asymmetrie war beim
        /// Geldansehen der Fehler – <c>AnsehenAktualisieren</c> gibt es nur auf <c>HumSpieler</c>.
        /// </summary>
        [Fact]
        public void Ein_KI_Angeklagter_profitiert_genauso_vom_Titel()
        {
            TestSpielwelt.Starte();

            var mensch = SW.Dynamisch.GetAktHum();
            mensch.SetAnsehen(0);
            mensch.SetTitel(Graf);

            int kiId = SW.Statisch.GetMinKIID() + 1;
            var ki = SW.Dynamisch.GetKIwithID(kiId);
            ki.SetMaennlich(mensch.GetMaennlich());
            ki.SetAnsehen(0);
            ki.SetTitel(Graf);

            Assert.Equal(mensch.GetStandesAnsehen(), ki.GetStandesAnsehen());

            // Derselbe Stand, dasselbe Plädoyer – unabhängig davon, wer auf der Anklagebank sitzt.
            string gegenMensch = Verteidigungsplaedoyer(1, SW.Statisch.GetMinKIID() + 2);
            string gegenKi = Verteidigungsplaedoyer(kiId, 1);

            Assert.Contains(PlaedoyerHoch, gegenKi);
            Assert.Equal(gegenMensch, gegenKi);

            // Ohne Titel fällt der KI-Angeklagte ebenso auf das schwache Plädoyer zurück.
            ki.SetTitel(Herr);
            Assert.Contains(PlaedoyerNiedrig, Verteidigungsplaedoyer(kiId, 1));
        }

        /// <summary>
        /// Die Geschworenen des Schuldenprozesses urteilen über eine Person wie das Gericht, also zählt
        /// auch dort der Stand. Der Aufbau ist bewusst deterministisch: alle KI-Beziehungen auf 0, damit
        /// im Urteilsterm nur noch Schulden, Zufall (0–15) und Ansehen stehen. Bei −300 Talern stimmt ein
        /// titelloser Spieler in 15 von 16 Zufallsfällen schuldig, ein Herzog (Bonus 150, mal 3 = 450)
        /// in keinem einzigen.
        /// </summary>
        [Fact]
        public void Der_Titel_traegt_auch_vor_den_Geschworenen()
        {
            Assert.True(ErmittleSchuldspruch(Herr), "Ohne Titel muss der Schuldenprozess schuldig sprechen.");
            Assert.False(ErmittleSchuldspruch(Herzog), "Ein Herzog muss durch seinen Stand freigesprochen werden.");
        }

        private static bool ErmittleSchuldspruch(int titel)
        {
            TestSpielwelt.Starte(seed: 4711);

            for (int id = SW.Statisch.GetMinKIID(); id < SW.Statisch.GetMaxKIID(); id++)
                SW.Dynamisch.GetKIwithID(id)?.SetBeziehungZuX(1, 0);

            var spieler = SW.Dynamisch.GetAktHum();
            spieler.SetTaler(-300);
            spieler.SetAnsehen(0);
            spieler.SetTitel(titel);

            // Den Würfelstrom des Prozesses selbst pinnen, damit beide Läufe dieselben Geschworenen
            // und dieselben Zufallswerte sehen.
            SW.Statisch.SetRnd(4711);

            return new ZugNachrichtenManager().FuehreSchuldenProzessDurch().Schuldig;
        }

        /// <summary>
        /// Die Gegenprobe und der eigentliche Sinn der Beschränkung: In der Wahl zählt weiterhin das
        /// blanke Ansehen. Kandidat 1 liegt mit seiner Beziehung (20 zu 10) vorn; der Titelbonus eines
        /// Herzogs wäre mit 150/10 = 15 Punkten groß genug, das zu drehen – er darf es nicht.
        /// </summary>
        [Fact]
        public void Der_Titel_kauft_keine_Wahl()
        {
            TestSpielwelt.Starte();

            int waehler = SW.Statisch.GetMinKIID() + 1;
            int kandidat1 = SW.Statisch.GetMinKIID() + 2;
            int kandidat2 = SW.Statisch.GetMinKIID() + 3;

            // Religionssympathie ausschalten (GetRelSympathieVonXzuY liefert für Konfessionsfreie 0).
            SW.Dynamisch.GetSpWithID(kandidat1).SetReligion(SW.Statisch.GetRelFreiID());
            SW.Dynamisch.GetSpWithID(kandidat2).SetReligion(SW.Statisch.GetRelFreiID());

            SW.Dynamisch.GetKIwithID(waehler).SetBeziehungZuX(kandidat1, 20);
            SW.Dynamisch.GetKIwithID(waehler).SetBeziehungZuX(kandidat2, 10);

            foreach (int id in new[] { kandidat1, kandidat2 })
            {
                SW.Dynamisch.GetSpWithID(id).SetAnsehen(0);
                SW.Dynamisch.GetSpWithID(id).SetTitel(Herr);
            }

            var kandidaten = new[] { kandidat1, kandidat2 };
            var manager = new AemterManager();

            Assert.Equal(0, manager.ErmittleKiStimme(waehler, kandidaten));

            // Der Herzogstitel darf am Wahlergebnis nichts ändern.
            SW.Dynamisch.GetSpWithID(kandidat2).SetTitel(Herzog);

            Assert.Equal(0, manager.ErmittleKiStimme(waehler, kandidaten));
        }

        /// <summary>
        /// Ebenfalls bewusst unverändert: Die Schuldentoleranz (<c>GetMaxSchulden − Ansehen * 10</c>)
        /// bemisst, wie tief jemand ins Minus darf – das ist keine Beurteilung einer Person. Ein
        /// Herzogstitel würde die Grenze um 1 500 Taler verschieben und den Prozess ganz verhindern.
        /// </summary>
        [Fact]
        public void Der_Titel_verschiebt_die_Schuldengrenze_nicht()
        {
            TestSpielwelt.Starte();

            var spieler = SW.Dynamisch.GetAktHum();
            spieler.SetAnsehen(0);
            spieler.SetTaler(SW.Statisch.GetMaxSchulden() - 1);

            var manager = new ZugNachrichtenManager();
            Assert.True(manager.MussSichVorGlaeubigernVerantworten());

            spieler.SetTitel(Herzog);
            Assert.True(manager.MussSichVorGlaeubigernVerantworten());
        }
    }
}
